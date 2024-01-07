// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class CookedMeat : GameItem {

    protected override void Start()
    {
        _itemName = "Cooked meat";
        _pickable = true;

        _objectId = ObjectId.CookedMeat;

        base.Start();
    }

    void Update()
    {

    }

    public override void OnAction(ItemAction action)
    {
        switch (action)
        {
            default:
                base.OnAction(action);
                break;
        }
    }
}
