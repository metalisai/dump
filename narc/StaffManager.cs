// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public enum StaffMemberType
{
    Unknown,
    Garderner,
    Dealer
}

// TODO: wtf is this class?
public class StaffManager : MonoBehaviour {

    public static StaffManager Instance;

    public void Start()
    {
        Instance = this;
    }
}
