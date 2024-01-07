// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public enum GardenerState
{
    Idle,
    MovingToHarvest,
    MovingToPlant // moving to plant a plant
}

[RequireComponent(typeof(PlaceableObject))]
public class GardenerAI : BaseAI {

    [HideInInspector]
    public Housing apartment;

    Store _store;
    GardenerObserver _observer;

	// Use this for initialization
	void Start () {
        GameTime.OnTick += Tick;

        // TODO: temporary
        apartment = GetComponent<PlaceableObject>().Infrastructure;
        GetComponentInParent<StaffMember>().Apartment = apartment;

        _store = FindObjectOfType<Store>();
        _observer = apartment.GardenerObserver;
    }

    // TODO: this is dumb and stupid and worst idea ever
    void FindBestJob()
    {
        if (_curJob == null)
        {
            _observer.AskJob(this);
        }
    }

    void Tick()
    {
        if (JobQueue.JobCount == 0)
        {
            FindBestJob();
        }
    }

    void OnDestroy()
    {
        GameTime.OnTick -= Tick;
    }
}
