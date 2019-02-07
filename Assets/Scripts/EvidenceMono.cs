﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvidenceMono : MonoBehaviour
{
    [SerializeField]
    private Evidence _evidenceInfo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Send an error if Evidence Info contains character info when it is not a conversation
        if (_evidenceInfo.GetEvidenceType != EvidenceType.Conversation && _evidenceInfo.Characters.Count > 0)
        {
            Debug.LogError("ERROR: Evidence type provides character information but is not a conversation!");
        }
    }

    public Evidence EvidenceInfo
    {
        get { return _evidenceInfo; }
    }
}
