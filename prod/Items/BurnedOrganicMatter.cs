// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

public class BurnedOrganicMatter : GameItem {

    protected override void Start()
    {
        _itemName = "Burned organic matter";
        _pickable = true;

        _objectId = ObjectId.BurnedOrganicMatter;

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
