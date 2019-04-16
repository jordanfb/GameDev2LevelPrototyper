﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YarnBoardCamera : MonoBehaviour
{
    public bool movementInverted = false;
    public float scrollSpeed = 1;
    public float moveSpeed = 1;

    [Space]
    public Vector2 minCoords = -Vector2.one;
    public Vector2 maxCoords = Vector2.one;
    public Vector2 zoomBounds = Vector2.up; // zoom is 0 to 1 by default

    private float zoom = 0;
    private float startingZoom;
    Vector3 startPos;
    public float duration = 0.5f;
    bool lookingAtEvidence = false;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startingZoom = transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.localPosition;

        float dzoom = Input.mouseScrollDelta.y * scrollSpeed;
        zoom += dzoom;
        zoom = Mathf.Clamp(zoom, zoomBounds.x, zoomBounds.y);
        newPos.z = startingZoom + zoom;
        if (!lookingAtEvidence)
        {
            // move around the world with middle mouse down
            if (Input.GetMouseButton(2))
            {
                // middle mouse down
                if (movementInverted)
                {
                    newPos -= (Vector3.right * Input.GetAxis("Mouse X") + Vector3.up * Input.GetAxis("Mouse Y")) * moveSpeed;
                }
                else
                {
                    newPos += (Vector3.right * Input.GetAxis("Mouse X") + Vector3.up * Input.GetAxis("Mouse Y")) * moveSpeed;
                }
                newPos.x = Mathf.Clamp(newPos.x, minCoords.x, maxCoords.x);
                newPos.y = Mathf.Clamp(newPos.y, minCoords.y, maxCoords.y);
            }
            startPos = transform.position; // set the zoom of the start pos probably?
        }
        transform.localPosition = newPos; // zoom towards the camera
    }

    public void LookAtEvidence(Transform target)
    {
        Vector3 endPos = new Vector3(transform.position.x, target.position.y, target.position.z);// - transform.forward*.7f; // move it back some
        lookingAtEvidence = true;
        StartCoroutine(CameraLerp(transform.position, endPos));
    }

    public void ReturnToStart()
    {
        lookingAtEvidence = false;
        StartCoroutine(CameraLerp(transform.position, startPos, true));
    }

    IEnumerator CameraLerp(Vector3 start, Vector3 end, bool setEndZoom = false)
    {
        for(float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, end, Smootherstep(t / duration));
            yield return 0;
        }

        transform.position = end;
        if (setEndZoom)
        {
            // then set the global zoom to be the end zoom!
            zoom = transform.localPosition.z - startingZoom;
        }
    }

    float Smootherstep(float x)
    {
        // adapted from wikipedia
        return x * x * x * (x * (x * 6 - 15) + 10);
    }
}
