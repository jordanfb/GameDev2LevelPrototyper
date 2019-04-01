﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simplePlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    ShadowRealmManager SRmanager;
    ThirdPersonCamera tpc;
    private Camera mainCam;

    public GameObject playerMesh;

    public List<Transform> playerHitPoints;

    private Rigidbody rb;
    [SerializeField]
    public float PLAYER_SPEED_FORWARD;
    public float PLAYER_SPEED_STRAFE;
    public float SHADOW_SPEED;

    private Animator anim;


    const float PLAYER_WIDTH = 0.5f;


    public Collider currentWallCollider = null;

    void Start()
    {
        SRmanager = GetComponent<ShadowRealmManager>();
        tpc = GetComponent<ThirdPersonCamera>();
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();
        anim = playerMesh.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isInShadowRealm", SRmanager.isInShadowRealm);
        if (SRmanager.isChoosingWall) {
            anim.SetFloat("yVelocity", 0f);
            anim.SetFloat("xVelocity", 0f);

            return;
        }


        float moveDirX = Input.GetAxis("Horizontal");
        float moveDirY = Input.GetAxis("Vertical");
        if (!SRmanager.isInShadowRealm) {
            thirdPersonMovement(moveDirX, moveDirY);
            //arbitrary speed in which player has to move, will need to change when I get Anims


        }
        else {
            wallMovement(moveDirX, moveDirY, currentWallCollider);
        }


    }

    void thirdPersonMovement(float _moveDirX, float _moveDirY) {
        if (_moveDirY < 0) {
            return;
        }
        if (_moveDirY > 0.1) {
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Scale(mainCam.transform.forward, new Vector3(1f, 0f, 1f)), Vector3.up);

            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, targetRot, 0.1f);


        }
        else if (Mathf.Abs(_moveDirX) > 0.1) {
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Scale(mainCam.transform.forward, new Vector3(1f, 0f, 1f)), Vector3.up);

            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, targetRot, 0.1f);

        }


        anim.SetFloat("yVelocity", transform.InverseTransformDirection(rb.velocity).z);
        anim.SetFloat("xVelocity", transform.InverseTransformDirection(rb.velocity).x);
        rb.velocity = ((new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z).normalized * _moveDirY * PLAYER_SPEED_FORWARD) 
                                    + (mainCam.transform.right.normalized * _moveDirX)* PLAYER_SPEED_STRAFE);





    }


    bool touchingWall = true;
    Quaternion targetWallRot = Quaternion.AngleAxis(90f, Vector3.up);
    void wallMovement(float _moveDirX, float _moveDirY, Collider _currentWallCollider) {

        if (_currentWallCollider == null) {
            Debug.Log("ERROR: current wall is null");
            return;

        }
        LayerMask mask = LayerMask.GetMask("WallLayer");
        RaycastHit hitWall;
        touchingWall = false;
        Vector3 nextPos;
        if (Vector3.Dot(SRmanager.shadowPlane.transform.right, mainCam.transform.forward) < -0.2f) {
            nextPos = SRmanager.shadowPlane.transform.position + SRmanager.shadowPlane.transform.forward * -_moveDirX * SHADOW_SPEED * Time.deltaTime
                            + (Mathf.Sign(_moveDirX) * SRmanager.shadowPlane.transform.forward * PLAYER_WIDTH);
        }
        else {
            nextPos = SRmanager.shadowPlane.transform.position + SRmanager.shadowPlane.transform.forward * -_moveDirX * SHADOW_SPEED * Time.deltaTime
                            - (Mathf.Sign(_moveDirX) * SRmanager.shadowPlane.transform.forward * PLAYER_WIDTH);
        }
        Debug.DrawRay(nextPos, SRmanager.shadowPlane.transform.right);
        if (Physics.Raycast(nextPos, SRmanager.shadowPlane.transform.right, out hitWall, Mathf.Infinity, mask))
        {
            if (hitWall.collider == _currentWallCollider) {
                touchingWall = true;

            }
           
        }



        if (touchingWall)
        {

            if(Vector3.Dot(SRmanager.shadowPlane.transform.right, mainCam.transform.forward) < -0.2f)
            {
                SRmanager.shadowPlane.transform.position -= SRmanager.shadowPlane.transform.forward * -_moveDirX * SHADOW_SPEED * Time.deltaTime;
            }
            else {
                SRmanager.shadowPlane.transform.position += SRmanager.shadowPlane.transform.forward * -_moveDirX * SHADOW_SPEED * Time.deltaTime;
            }

            anim.SetFloat("xVelocityShadow", Mathf.Abs(-_moveDirX * SHADOW_SPEED) * Time.deltaTime);

            if (_moveDirX>0) {
                targetWallRot = Quaternion.AngleAxis(90f, Vector3.up);

            }
            else if (_moveDirX < 0)
            {
                targetWallRot = Quaternion.AngleAxis(-90f, Vector3.up);

            }

            float turnSpeed = 0.1f;
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, targetWallRot, turnSpeed);

        }
        else {

            anim.SetFloat("xVelocityShadow", 0f);
        }








    }


    public Collider setCurrentWallCollider(Collider newCurrentWallCollider) {
        currentWallCollider = newCurrentWallCollider;
        return currentWallCollider;

    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Evidence" && Input.GetKeyDown(KeyCode.E))
        {
            EvidenceMono emono = other.gameObject.GetComponent<EvidenceMono>();
            emono.CollectThisEvidence();
            //Evidence e = emono.EvidenceInfo;
            //PlayerManager.instance.CollectEvidence(e);
            StartCoroutine(DestroyAfterTime(1f, other.gameObject));
            anim.SetTrigger("pickUp");

        }
    }

    IEnumerator DestroyAfterTime(float time, GameObject gameObjectToDestroy)
    {
        yield return new WaitForSeconds(time);

        Destroy(gameObjectToDestroy);
    }
}
