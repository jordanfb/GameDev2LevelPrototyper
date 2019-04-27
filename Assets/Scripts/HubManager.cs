﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    /*
     * Things to do:
     * lerp camera to and from desk
     * eventually remove the donotexitfromdesk collider once the desk has an actual collider
     * click the gun makes UI show up for picking a target
     * 
     * replace the gotoscenes in the gameplaymanager with the real scene names
     * add the yarnboard to this
     *  - make the yarnboard work
     * deal with the player camera wanting to follow the player but being required to lerp to the yarnboard or the desk
     * save and load the text for what happened each day in playerprefs (steal code from my yarnboard stuff)
     */
    public GameObject cameraGameObject;
    public ThirdPersonCamera characterCamera;
    public Transform DeskCameraLocation;
    public Transform YarnBoardCameraLocation;
    private float cameraLerpPos = .01f; // 0 is the player
    public CameraMode cameraMode = CameraMode.FollowPlayer; // looking at desk, player, or yarnboard

    [Header("Player")]
    public simplePlayerMovement player; // used for entering colliders to see the desk

    [Header("Desk Item Variables")]
    public Transform deskItemParent;
    public Vector3 deskItemFinalOffset;
    public List<DeskDayDescriptionItem> deskItems; // in order by days!
    public float deskItemsLerpSpeed = 1;
    public Vector3 deskItemCameraOffset = Vector3.down * .5f;
    public GameObject deskCollider;
    public BoxCollider enterDeskCollider;

    [Header("Gun variables")]
    public Transform gunTransform;
    public Transform finalGunPosition;
    public Material GlowingGunMaterial; // for the final day it's highlighted
    public EndgameManager endgameManager;

    [Header("Yarn Board")]
    public GameObject yarnBoardExitCollider; // for exiting the yarnboard via click
    public BoxCollider enterYarnCollider;
    public YarnBoard yarnBoard;

    public bool isDemoHubworld = false;


    private Transform otherCameraTransformPosition;

    // Start is called before the first frame update
    void Start()
    {
        // it needs to load all the summaries of the days here
        // it gets the info from the static gameplaymanager
        LoadDesk();
        otherCameraTransformPosition = DeskCameraLocation;

        // destroy the level 1 custom logic
        LevelOneEvidenceManager level1CustomLogic = FindObjectOfType<LevelOneEvidenceManager>();
        if (level1CustomLogic != null)
        {
            Destroy(level1CustomLogic.gameObject);
            LevelOneEvidenceManager.instance = null;
        }
    }

    public void ExitDesk()
    {
        enterDeskCollider.gameObject.GetComponent<Interactable>().hubFocus = false;

        // this is called when the collider is clicked probably
        cameraMode = CameraMode.FollowPlayer; // go back to following the player
        for (int i = 0; i < deskItems.Count; i++)
        {
            deskItems[i].ResetPosition(); // they should go back to the table not nearby the camera duh
        }

        LockMouse.LockTheMouse();
        endgameManager.gameObject.SetActive(false);
        deskCollider.SetActive(false);
        player.isAllowedToWalk = true;
    }

    public void EnterDesk()
    {
        enterDeskCollider.gameObject.GetComponent<Interactable>().hubFocus = true;
        player.isAllowedToWalk = false;
        cameraMode = CameraMode.LookAtDesk;
        deskCollider.SetActive(true);
        LockMouse.UnlockTheMouse();
        otherCameraTransformPosition = DeskCameraLocation;
    }

    public void EnterYarnBoard()
    {
        enterYarnCollider.gameObject.GetComponent<Interactable>().hubFocus = true;
        player.isAllowedToWalk = false;
        cameraMode = CameraMode.LookAtYarnBoard;
        yarnBoardExitCollider.SetActive(false);
        LockMouse.UnlockTheMouse();
        otherCameraTransformPosition = YarnBoardCameraLocation;
    }
    
    public void ExitYarnBoard()
    {
        //print("exit yarn board");
        enterYarnCollider.gameObject.GetComponent<Interactable>().hubFocus = false;
        // this is called when the collider is clicked probably
        player.isAllowedToWalk = true;
        // Remove YarnBoard UI from screen so we can do things!
        if (yarnBoard._suspectInfoPanel.activeInHierarchy)
            yarnBoard.DeactivateSuspectPanel();
        if (yarnBoard._flavorTextPanel.activeInHierarchy)
            yarnBoard.DeactivateFlavorText();

        cameraMode = CameraMode.FollowPlayer; // go back to following the player
        yarnBoardExitCollider.SetActive(false);
        LockMouse.LockTheMouse();
        EvidenceManager.SaveEvideneToPlayerPrefs(); // save the evidence after you leave the yarnboard.
    }

    // Update is called once per frame
    void Update()
    {
        // only have these cheat keys if we're in the editor, not a build FIX
        if (GameplayManager.instance.debugMode && Input.GetKeyDown(KeyCode.M))
        {
            // skip a day
            GameplayManager.instance.SkipDay("Visited stuff saw death la-di-da\n\nOh yeah and I was caught");
            LoadDesk();
        }
        if (GameplayManager.instance.debugMode && Input.GetKeyDown(KeyCode.N))
        {
            // skip a day
            GameplayManager.instance.NextDay("Visited stuff found stuff whoop-di-do");
            LoadDesk();
        }

        if (cameraMode == CameraMode.LookAtDesk && Input.GetKeyDown(KeyCode.E))
        {
            // then if they press E exit the desk or exit the yarnboard
            ExitDesk();
        }
        else if (cameraMode == CameraMode.LookAtYarnBoard && Input.GetKeyDown(KeyCode.E))
        {
            ExitYarnBoard();
        } else if (cameraMode == CameraMode.FollowPlayer && Input.GetKeyDown(KeyCode.E))
        {
            // then check if the player is inside a collider or whatever
            //if (player.gameObject.GetComponent<Collider>())
            float distanceToDesk = Vector3.Distance(player.transform.position, enterDeskCollider.transform.position);
            float distanceToYarn = Vector3.Distance(player.transform.position, enterYarnCollider.transform.position);
            if (distanceToDesk < distanceToYarn)
            {
                // enter the desk
                EnterDesk();
            } else
            {
                EnterYarnBoard();
            }
        }
    }

    private void LateUpdate()
    {
        // update the yarnboard undo/redo stack as well
        if (cameraMode == CameraMode.LookAtYarnBoard)
        {
            // then check for control+z or control+shift+z or control+y
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        // redo
                        UndoRedoStack.Redo();
                        yarnBoard.GenerateContent();
                    }
                    else
                    {
                        UndoRedoStack.Undo();
                        yarnBoard.GenerateContent();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Y)) {
                    // redo
                    UndoRedoStack.Redo();
                    yarnBoard.GenerateContent();
                }
            }
        }
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        bool updatePos = false;
        if (cameraMode == CameraMode.FollowPlayer && cameraLerpPos > 0)
        {
            updatePos = true;
            cameraLerpPos -= Time.deltaTime;
            if (cameraLerpPos <= 0)
            {
                cameraLerpPos = 0;
                cameraGameObject.transform.SetParent(characterCamera.mainCam.transform);
            }
        }
        else if (cameraLerpPos < 1)
        {
            updatePos = true;
            cameraLerpPos += Time.deltaTime;
            if (cameraLerpPos >= 1)
            {
                cameraGameObject.transform.SetParent(otherCameraTransformPosition);
                cameraLerpPos = 1;
            }
        }


        // now lerp the camera using smootherstep
        // now lerp between them
        if (updatePos)
        {
            // only update the pos if the camera lerp has moved
            float t = DeskDayDescriptionItem.Smootherstep(cameraLerpPos);
            cameraGameObject.transform.position = Vector3.Lerp(characterCamera.mainCam.transform.position, otherCameraTransformPosition.position, t);
            cameraGameObject.transform.rotation = Quaternion.Lerp(characterCamera.mainCam.transform.rotation, otherCameraTransformPosition.rotation, t);
        }
    }

    public void ClickOnGun()
    {
        if (GameplayManager.instance.IsChoosingDay() && cameraMode == CameraMode.LookAtDesk)
        {
            // only do anything if you click on it at the right time
            //Debug.Log("Clicked on the gun");
            endgameManager.gameObject.SetActive(true);
        }
    }

    private void LoadDesk()
    {
        // load the desk UI stuff here
        // setting the text and whatever
        string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday\nLast Day" };
        for (int i = 0; i < deskItems.Count; i++)
        {
            string content = "<size=.05><u>" + dayNames[i] + "</u></size>\n";
            if (GameplayManager.instance.dayData.Count > i)
            {
                content += GameplayManager.instance.dayData[i].dayContent;
            }
            if (i == GameplayManager.instance.dayNum)
            {
                // activate that day's buttons
                deskItems[i].EnableButtons();
                content += "<size=.03>Where should I look for evidence?</size>";
            } else
            {
                // deactivate them
                deskItems[i].DisableButtons();
            }
            deskItems[i].SetContents(content);
        }

        // if it's the last day then move the desk item parent up,
        // move the gun down, and enable the gun and stuff like that
        if (GameplayManager.instance.IsChoosingDay())
        {
            // then move everything
            deskItemParent.position += deskItemFinalOffset;
            for(int i = 0; i < deskItems.Count; i++)
            {
                deskItems[i].SetOriginalPositions(); // make sure it knows where to return to now
            }
            gunTransform.position = finalGunPosition.position;
            gunTransform.rotation = finalGunPosition.rotation;
            // also enable the gun clicking but that's the next problem
            gunTransform.GetComponent<MeshRenderer>().material = GlowingGunMaterial;
            // also enable the clicking on script
            //gunTransform.GetComponent<>
        }
    }

    [ContextMenu("To Desk Camera")]
    public void ToDeskLocation()
    {

        Camera.main.transform.position = DeskCameraLocation.position;
        Camera.main.transform.rotation = DeskCameraLocation.rotation;
    }

    [ContextMenu("To Yarn Board Camera")]
    public void ToYarnboardLocation()
    {
        Camera.main.transform.position = YarnBoardCameraLocation.position;
        Camera.main.transform.rotation = YarnBoardCameraLocation.rotation;
    }

    public enum CameraMode
    {
        FollowPlayer, LookAtDesk, LookAtYarnBoard
    }
}
