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
    bool canExit = true; // to stop it from leaving twice

    public Animator anim;

    public bool isAllowedToWalk = true;

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
    Vector3 camForwardOnEnteranceToShadowRealm;
    void Update()
    {
        anim.SetBool("isInShadowRealm", SRmanager.isInShadowRealm);
        if (SRmanager.isChoosingWall) {
            anim.SetFloat("yVelocity", 0f);
            anim.SetFloat("xVelocity", 0f);

            return;
        }

        if (isAllowedToWalk)
        {
            float moveDirX = Input.GetAxis("Horizontal");
            float moveDirY = Input.GetAxis("Vertical");
            if(System.Math.Abs(moveDirY) < 0.01f && System.Math.Abs(moveDirY) < 0.01f) {
                Camera.main.GetComponent<LozowichEffect>().animateTexture = 0;
            }
            else {
                Camera.main.GetComponent<LozowichEffect>().animateTexture = 1;
            }
            if (!SRmanager.isInShadowRealm)
            {
                thirdPersonMovement(moveDirX, moveDirY);
                //arbitrary speed in which player has to move, will need to change when I get Anims
            }
            else
            {
                if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) {
                    camForwardOnEnteranceToShadowRealm = mainCam.transform.forward;
                }
                wallMovement(moveDirX, moveDirY, currentWallCollider, camForwardOnEnteranceToShadowRealm);
            }
        }
    }

    public Animator getAnim()
    {

        return anim;
    }

    void thirdPersonMovement(float _moveDirX, float _moveDirY) {
        //if (_moveDirY < 0) {
        //    return;
        //}
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
        rb.velocity = ((new Vector3(mainCam.transform.forward.x, 0, mainCam.transform.forward.z).normalized * _moveDirY * PLAYER_SPEED_FORWARD)
                                    + (mainCam.transform.right.normalized * _moveDirX) * PLAYER_SPEED_STRAFE) + rb.velocity.y * transform.up;





    }


    bool touchingWall = true;
    Quaternion targetWallRot = Quaternion.AngleAxis(90f, Vector3.up);
    void wallMovement(float _moveDirX, float _moveDirY, Collider _currentWallCollider, Vector3 cameraForwardOnEnterance) {

        if (_currentWallCollider == null) {
            Debug.Log("ERROR: current wall is null");
            return;

        }
        LayerMask mask = LayerMask.GetMask("WallLayer");
        RaycastHit hitWall;
        touchingWall = false;




        Vector3 nextPos;
        if (Vector3.Dot(SRmanager.shadowPlane.transform.right, cameraForwardOnEnterance) < -0.2f) {
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

            if(Vector3.Dot(SRmanager.shadowPlane.transform.right, cameraForwardOnEnterance) < -0.2f)
            {
                SRmanager.shadowPlane.transform.position -= SRmanager.shadowPlane.transform.forward * -_moveDirX * SHADOW_SPEED * Time.deltaTime;
            }
            else {
                SRmanager.shadowPlane.transform.position += SRmanager.shadowPlane.transform.forward * -_moveDirX * SHADOW_SPEED * Time.deltaTime;
            }

            anim.SetFloat("xVelocityShadow", Mathf.Abs(-_moveDirX * SHADOW_SPEED) * Time.deltaTime);

            if (_moveDirX * Vector3.Dot(SRmanager.shadowPlane.transform.right, cameraForwardOnEnterance) > 0) {
                targetWallRot = Quaternion.AngleAxis(90f, Vector3.up);

            }
            else if (_moveDirX * Vector3.Dot(SRmanager.shadowPlane.transform.right, cameraForwardOnEnterance) < 0)
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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Exit" && canExit)
        {
            canExit = false;
            GameplayManager.instance.ExitBackToHubNextDay();
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
            EvidenceMono emono = other.transform.parent.gameObject.GetComponentInChildren<EvidenceMono>();
            emono.CollectThisEvidence();
            //Evidence e = emono.EvidenceInfo;
            //PlayerManager.instance.CollectEvidence(e);
            StartCoroutine(DestroyAfterTime(1f, other.transform.parent.gameObject));
            Destroy(other.gameObject.GetComponent<Interactable>().interactTextInGame);
            if (emono.isWaistLevel) {
                anim.SetTrigger("reachOver");
            }
            else {
                anim.SetTrigger("pickUp");
            }


        }
    }

    IEnumerator DestroyAfterTime(float time, GameObject gameObjectToDestroy)
    {
        yield return new WaitForSeconds(time);

        Destroy(gameObjectToDestroy);
    }


}
