﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YarnBoard : MonoBehaviour
{
    [SerializeField]
    private GameObject _pinPrefab;
    [SerializeField]
    private GameObject _boardCanvas;
    [SerializeField]
    private GameObject _flavorTextPanel;
    [SerializeField]
    private Text _evidenceTitle;
    [SerializeField]
    private Text _flavorTextAsset;
    [SerializeField]
    private float _pinZOffset = 0.0f;
    [SerializeField]
    private float _offsetRatio;
    [Tooltip("Determines the x offset of character text meshes on the notebook paper.")]
    [SerializeField]
    private float _textPositionXOffset;
    [SerializeField]
    private int _charTextFontSize;
    [SerializeField]
    private YarnBoardCamera _yarnBoardCamera;
    [SerializeField]
    private Sprite _placeholderSprite;
    [SerializeField]
    private float _photoScalar;
    [SerializeField]
    private Transform _yarnBoardParent;
    [SerializeField]
    private float _evidenceXOffset;
    [SerializeField]
    private float _evidenceYOffset;
    [SerializeField]
    private float _pinScale;
    [SerializeField]
    private float _pinOffset;


    private List<GameObject> _pins;
    private YarnBoardMode mode = YarnBoardMode.None;
    private Collider _collider;
    private Vector2 _minPinPos;
    private Vector2 _maxPinPos;

    // evidene moving info:
    private GameObject movingYarnboardItem;

    public enum YarnBoardMode
    {
        None, Displaying, Moving
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateContent(); // load the content from the evidence manager
        /*
        //For use with randomly placing pins
        _collider = GetComponent<Collider>();
        _minPinPos = new Vector2(_collider.bounds.min.x, _collider.bounds.min.y);
        _maxPinPos = new Vector2(_collider.bounds.max.x, _collider.bounds.max.y);

        //Initialize Pins
        _pins = new List<GameObject>();
        foreach (Evidence evidence in PlayerManager.instance.CollectedEvidence)
        {
            GameObject pin = Instantiate(_pinPrefab) as GameObject;
            // set the parent as the parent
            // of the yarn board so we can move the whole thing around
            RandomizePositionOnBoard(ref pin, _pins.IndexOf(pin));
            _pins.Add(pin);

            switch (evidence.GetEvidenceType)
            {
                case EvidenceType.Object:
                    //Display photo associated with evidence SO & parent it to pin
                    GameObject photo = new GameObject(evidence.Name);
                    photo.tag = "Evidence";
                    photo.transform.parent = pin.transform;
                    photo.AddComponent<SpriteRenderer>().sprite = evidence.Photo;
                    BoxCollider pColl = photo.AddComponent<BoxCollider>();
                    photo.transform.localPosition = new Vector3(0f, 1f, PinOffset(pColl));
                    photo.AddComponent<EvidenceMono>().EvidenceInfo = evidence;
                    break;
                case EvidenceType.Conversation:
                    //Create a generic piece of paper sprite
                    GameObject paper = new GameObject(evidence.Name);
                    paper.tag = "Evidence";
                    paper.transform.parent = pin.transform;
                    paper.AddComponent<SpriteRenderer>().sprite = evidence.Photo;
                    BoxCollider paColl = paper.AddComponent<BoxCollider>();
                    //PINOFFSET SET TO Z BC PINS ARE ROTATED
                    paper.transform.localPosition = new Vector3(0f, 1f, PinOffset(paColl)); 
                    paper.AddComponent<EvidenceMono>().EvidenceInfo = evidence;
                    //Create text object to display over paper
                    GameObject text = new GameObject(evidence.Name + "Characters");
                    text.transform.parent = paper.transform;
                    text.transform.localPosition = new Vector3(0f, 0f, -1f);
                    DisplayCharacters(ref text, evidence);
                    break;
                case EvidenceType.Document:
                    //Display the gameObject as it exists in the world
                    //Later on we might have to rotate it to face the camera
                    //Because we are instantiating a stored go and not making a new one, 
                    //adding a collider and EvidenceMono is unnecessary.
                    GameObject document = new GameObject(evidence.Name);
                    document.tag = "Evidence";
                    document.name = evidence.Name;
                    document.AddComponent<EvidenceMono>().EvidenceInfo = evidence;
                    document.transform.parent = pin.transform;
                    document.AddComponent<SpriteRenderer>().sprite = evidence.Photo;
                    // CHANGE WHEN ACTUAL DOCUMENT MODELS ARE AVAILABLE
                    BoxCollider dColl = document.AddComponent<BoxCollider>();
                    document.transform.localPosition = new Vector3(0f, 6f, PinOffset(dColl));
                    break;
            }

        }
        */
    }

    public void GenerateContent()
    {
        for (int i = 0; i < EvidenceManager.AllEvidence.Count; i++)
        {
            // spawn the evidence that's on the board or off it, and do nothing to the rest
            SerializedEvidence se = EvidenceManager.AllEvidence[i];
            YarnBoardEntity ybe = EvidenceManager.instance.ReferencedEntity(se);

            // We're only worried about evidence we have available for the yarn board
            if (se.evidenceState == SerializedEvidence.EvidenceState.NotInGame || se.evidenceState == SerializedEvidence.EvidenceState.NotFound)
                continue;

            Evidence e = ybe as Evidence;
            Suspect s = ybe as Suspect;
            GameObject go = null;
            if (e != null)
            {
                // it's regular evidence
                go = new GameObject(e.Name);
                go.transform.localScale *= _photoScalar;
                //go.transform.parent = _yarnBoardParent;
                go.transform.Rotate(0f, 90f, 0f);
                go.transform.position = _yarnBoardParent.position;
                go.AddComponent<SpriteRenderer>().sprite = _placeholderSprite;
                go.AddComponent<BoxCollider>();
                go.AddComponent<EvidenceMono>().EvidenceInfo = e;
                go.tag = "Evidence";


            } else if (s != null)
            {
                // then create a suspect evidence thing
                go = new GameObject(s.CodeName);
                go.transform.localScale *= _photoScalar;
                //go.transform.parent = _yarnBoardParent;
                go.transform.Rotate(0f, -90f, 0f);
                go.transform.position = new Vector3(_evidenceXOffset, _evidenceYOffset, _yarnBoardParent.position.z);
                go.AddComponent<SpriteRenderer>().sprite = _placeholderSprite;
                go.AddComponent<BoxCollider>();
                go.AddComponent<SuspectMono>().SuspectInfo = s;
                go.tag = "Evidence";
            }

            if (go == null)
                continue;

            if (se.evidenceState == SerializedEvidence.EvidenceState.OnYarnBoard)
            {
                go.transform.position = new Vector3(se.location.x, se.location.y, this.gameObject.transform.position.z);
            }

            GameObject pin = Instantiate(_pinPrefab, go.transform);
            pin.transform.localPosition = new Vector3(0f, _pinOffset, 0f);
            pin.transform.localScale = new Vector3(_pinScale, _pinScale, _pinScale);
            pin.transform.Rotate(0f, 0f, 180f);
            //GameObject pinChild = pin.transform.GetChild(0).gameObject;
            //pinChild.transform.Rotate(0f, 0f, 180f);
            
        }
    }

    private float PinOffset(Collider c)
    {
        return (1f / _offsetRatio) * (c.bounds.max.y - c.bounds.center.y);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.T))
        {
            System.Random rng = new System.Random();
            int next = rng.Next(0, EvidenceManager.AllEvidence.Count - 1);
            EvidenceManager.AllEvidence[next].evidenceState = SerializedEvidence.EvidenceState.OffYarnBoard;
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Evidence")) 
                Destroy(go);
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Suspect"))
                Destroy(go);
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Pin"))
                Destroy(go);
            GenerateContent();
        }

#endif
            if (Input.GetMouseButtonDown(0) && mode == YarnBoardMode.None)
        {
            //Raycast to screen
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // On hit, check if it's a piece of evidence
            if (Physics.Raycast(ray, out hit))
            {
                GameObject evidence = hit.collider.gameObject;
                if (evidence.tag == "Evidence")
                {
                    _yarnBoardCamera.LookAtEvidence(evidence.transform);
                    _flavorTextPanel.SetActive(true);
                    _flavorTextAsset.text = evidence.GetComponent<EvidenceMono>().EvidenceInfo.FlavorText;
                    _evidenceTitle.text = evidence.GetComponent<EvidenceMono>().EvidenceInfo.Name;
                    //displaying = true;
                    mode = YarnBoardMode.Displaying;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && mode == YarnBoardMode.None)
        {
            // move the item you clicked on
            Debug.Log("Mtrying to oving now");
            //Raycast to screen
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // On hit, check if it's a piece of evidence
            if (Physics.Raycast(ray, out hit))
            {
                GameObject evidence = hit.collider.gameObject;
                if (evidence.tag == "Evidence")
                {
                    Debug.Log("Moving now");
                    // get the correct object to move
                    movingYarnboardItem = hit.collider.transform.parent.gameObject;
                    mode = YarnBoardMode.Moving;
                }
            }
        }
        else if (Input.GetMouseButton(1) && mode == YarnBoardMode.Moving)
        {
            Debug.Log("clicking annd holding");

            // move the item you clicked on

            //Raycast to screen
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // On hit, check if it's a piece of evidence
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("YarnBoard")))
            {
                movingYarnboardItem.transform.position = hit.point;
            }
        }
        else if (Input.GetMouseButtonUp(1) && mode == YarnBoardMode.Moving)
        {
            // move the item you clicked on
            Debug.Log("nolonger clicking");

            //Raycast to screen
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // On hit, check if it's a piece of evidence
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("YarnBoard")))
            {
                movingYarnboardItem.transform.position = hit.point;
            }
            mode = YarnBoardMode.None;
        }
    }

    private void RandomizePositionOnBoard(ref GameObject pin, int index)
    {
        float randX = Random.Range(_minPinPos.x * .75f, _maxPinPos.x * .75f);
        float randY = Random.Range(_minPinPos.y * .75f, _maxPinPos.y * .75f);
        pin.transform.position = new Vector3(randX + index, randY, transform.position.z - _pinZOffset);
        pin.GetComponentInChildren<Transform>().position = Vector3.zero;
    }

    //Isolated for readability sake
    private void DisplayCharacters(ref GameObject go, Evidence ev)
    {
        //Compile a string with all characters
        string charText = "";
        foreach(string ch in ev.Characters)
        {
            charText += (ch + "\n");
        }

        //Create a new textmesh with parameters
        TextMesh t = go.AddComponent<TextMesh>();
        t.text = charText;
        t.color = Color.black;
        t.fontSize = _charTextFontSize;
    }

    public void DeactivateFlavorText()
    {
        _flavorTextPanel.SetActive(false);
        _flavorTextAsset.text = "";
        //displaying = false;
        mode = YarnBoardMode.None;
    }
}
