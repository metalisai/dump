// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class ReplantJob : AIJob
{
    bool _done = false;
    bool _success = false;
    GridCell _target;

    public ReplantJob(GridCell target)
    {
        _target = target;
    }

    public override bool IsJobDone(ref bool success)
    {
        success = _success;
        return _done;
    }

    public override void JobTick()
    {
        
    }

    public override void StartJob()
    {
        var store = GameObject.FindObjectOfType<Store>();
        var apartment = _target.Grid.Infrastructure;

        if (store.BuyItem(store.GetStoreItemOfItem(PlaceableObjectType.WeedPlant)))
        {
            apartment.AddObjectToPosition(PlaceableObjectType.WeedPlant, _target.Center);
            _success = true;
        }
        else // buying failed, so cancel the job and requeue it
        {
            apartment.GardenerObserver.QueueForReplant(_target);
            _success = false;
        }
        _done = true;
    }
}
