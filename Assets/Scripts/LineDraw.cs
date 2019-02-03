﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour
{
    private LineRenderer _line;
    private Vector3 _mousePos;
    private GameObject _startPin;
    private int _currentLines = 0;

    public Material material;
    public float _lineWidth = 0.15f;

    // Update is called once per frame
    void Update()
    {
        // Left click creates a new line if clicking a pin.
        if(Input.GetMouseButtonDown(0))
        {
            if(_line == null)
            {
                //Cast a ray from the mouse to the screen
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                //If it's a hit, check to see if the gameobject is tagged as a pin
                if(Physics.Raycast(ray, out hit))
                {
                    Collider coll = hit.collider;
                    if(coll.gameObject.tag == "Pin")
                    {
                        //Create a line if it's a pin object
                        _startPin = coll.gameObject;
                        CreateLine(coll.gameObject.transform.position);
                    }
                }
            }

        }
        //Once the held mouse button is released
        //BUG: Need to store start pin so yarn doesn't connect with itself
        else if(Input.GetMouseButtonUp(0) && _line)
        {
            //Cast a ray again from mouse to screen
            RaycastHit endHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out endHit))
            {
                Collider endColl = endHit.collider;
                if(endColl.gameObject.tag == "Pin" && endColl.gameObject != _startPin)
                {
                    //Set the endpoint position of the line to the end pin
                    _line.SetPosition(1, endColl.gameObject.transform.position);
                    AddCollider();
                    _currentLines++;
                }
                else
                {
                    //Line is invalid, destroy
                    Destroy(_line.gameObject);
                }
            }
            _line = null;
            _startPin = null;
        }
        //An attempt at dragging the line, but without a raycast it doesn't really work
        /*
        else if(Input.GetMouseButton(0) && _line != null) 
        {
            _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _mousePos.z = _line.GetPosition(0).z;

            _line.SetPosition(1, _mousePos);
        }*/

        //Delete a line on right click
        if(Input.GetMouseButtonDown(1))
        {
            RaycastHit delHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out delHit))
            {
                Collider delColl = delHit.collider;
                if(delColl.gameObject.tag == "Line")
                {
                    Destroy(delColl.transform.parent.gameObject);
                }
            }
        }
    }

    /**
     * Creates a line with vertices at the transform of the starting pin.
     * Requires line to be null, otherwise you could take control of multiple lines at once.
     */
    private void CreateLine(Vector3 position) // Can probably remove the vector3 arg now that I track the starting pin
    {
        if (_line != null) return;
        _line = new GameObject("Line" + _currentLines).AddComponent<LineRenderer>();
        //Eventually this material will be an actual yarn texture
        _line.material = material;
        _line.positionCount = 2;
        _line.startWidth = _lineWidth;
        _line.useWorldSpace = true;
        //Set both vertices to the start pin
        _line.SetPosition(0, position);
        _line.SetPosition(1, position);
    }

    /**
     * Creates a GameObject with a box collider that wraps around a
     * newly connected line.
     * Because I don't track the end pin, we need to pass it in.
     * */
    private void AddCollider()
    {
        BoxCollider lineColl = new GameObject("LineCollider" + _currentLines).AddComponent<BoxCollider>();
        lineColl.transform.parent = _line.transform;
        lineColl.gameObject.tag = "Line";

        float lineWidth = _line.startWidth;
        float lineLength = Vector3.Distance(_line.GetPosition(0), _line.GetPosition(1));

        lineColl.size = new Vector3(lineLength, lineWidth, 1f);

        Vector3 midPoint = (_line.GetPosition(0) + _line.GetPosition(1)) / 2;
        lineColl.transform.position = midPoint;

        float angle = Mathf.Atan2((_line.GetPosition(1).z - _line.GetPosition(0).z), (_line.GetPosition(1).x - _line.GetPosition(0).x));
        angle *= -Mathf.Rad2Deg;
        lineColl.transform.Rotate(0f, angle, 0f);
    }
}
