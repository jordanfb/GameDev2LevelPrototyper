﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Evidence", menuName = "Evidence", order = 1)]
public class Evidence : ScriptableObject
{
    [SerializeField]
    private string _name;
    //For use with Objects mostly
    //If used with Conversations, drag in the default notebook paper sprite
    //(or whatever it's called
    [SerializeField]
    private Sprite _photo;
    //WRITERS: fill this in with whatever information is important
    [SerializeField]
    [TextArea]
    private string _flavorText;
    //All evidence MUST have a type
    [SerializeField]
    private EvidenceType _evidenceType;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public Sprite Photo
    {
        get { return _photo; }
        set { _photo = value; }
    }

    public string FlavorText
    {
        get { return _flavorText; }
    }

    public EvidenceType GetEvidenceType
    {
        get { return _evidenceType; }
    }

    
}

public enum EvidenceType
{
    Object, Conversation, Document
}
