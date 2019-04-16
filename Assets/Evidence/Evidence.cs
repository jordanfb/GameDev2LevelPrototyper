﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Evidence", menuName = "Evidence", order = 1)]
public class Evidence : YarnBoardEntity
{
    //WRITERS: fill this in with whatever information is important
    [SerializeField]
    [TextArea]
    private string _flavorText;
    //All evidence MUST have a type
    [SerializeField]
    private EvidenceType _evidenceType;
    [SerializeField]
    private Level _level;
    [SerializeField]
    private List<Suspect> _associatedSuspects;
    [SerializeField]
    private List<string> _characters;

    public string FlavorText
    {
        get { return _flavorText; }
    }

    public EvidenceType GetEvidenceType
    {
        get { return _evidenceType; }
    }

    public Level GetLevel
    {
        get { return _level; }
    }

    public List<Suspect> AssociatedSuspects
    {
        get { return _associatedSuspects; }
    }

    public List<string> Characters
    {
        get { return _characters; }
    }
}

public enum EvidenceType
{
    Object, Conversation, Document
}

public enum Level
{
    Office, Factory, Apartment, Hub
}
