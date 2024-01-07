// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class Weed {

    public WeedState State { get; set; }
}

public enum WeedState
{
    Growing,
    Drying,
    Dryed
}