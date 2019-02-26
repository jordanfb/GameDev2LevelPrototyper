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
    private Animator anim;


    const float PLAYER_WIDTH = 0.5f;
    const float WALL_SPEED = 0.1f;

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
    void wallMovement(float _moveDirX, float _moveDirY, Collider _currentWallCollider) {

        if (_currentWallCollider == null) {
            Debug.Log("ERROR: current wall is null");
            return;

        }
        LayerMask mask = LayerMask.GetMask("WallLayer");
        RaycastHit hitWall;
        touchingWall = false;
        Vector3 nextPos = SRmanager.shadowPlane.transform.position + SRmanager.shadowPlane.transform.forward * -_moveDirX * WALL_SPEED 
                            - (Mathf.Sign(_moveDirX)* SRmanager.shadowPlane.transform.forward * PLAYER_WIDTH);
        Debug.DrawRay(nextPos, SRmanager.shadowPlane.transform.right);
        if (Physics.Raycast(nextPos, SRmanager.shadowPlane.transform.right, out hitWall, Mathf.Infinity, mask))
        {
            if (hitWall.collider == _currentWallCollider) {
                touchingWall = true;

            }
           
        }

        if (touchingWall)
        {

            SRmanager.shadowPlane.transform.position += SRmanager.shadowPlane.transform.forward * -_moveDirX * 0.1f;


        }
        







    }


    public Collider setCurrentWallCollider(Collider newCurrentWallCollider) {
        currentWallCollider = newCurrentWallCollider;
        return currentWallCollider;

    }

}
