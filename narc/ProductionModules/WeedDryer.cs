// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WeedDryer : PlaceableObject
{

    public static float DryingTime = 5f;
    public int Slots = 10;
    public GameObject[] ControlledObjects;

    public WeedDryerState State { get { return _state; }}

    private WeedDryerSlot[] _slots;
    private WeedDryerControlledObject[] _controlledObjects;
    private WeedDryerState _state = WeedDryerState.Empty;

	public WeedDryer ()
	{
		//_type = ModuleType.WeedDryer;
	}

    void Start()
    {

        _slots = new WeedDryerSlot[Slots];

        int objects = ControlledObjects.Length;

        if(objects == 0)
            Debug.LogError("Assign controlled objects to WeedDryer!!!!");

        int slotsPerObject = Slots/objects;

        _controlledObjects = new WeedDryerControlledObject[ControlledObjects.Length];

        int j = 0;
        for (int i = 0; i < _controlledObjects.Length; i++)
        {
            ControlledObjects[i].SetActive(false);

            _controlledObjects[i] = new WeedDryerControlledObject
            {
                ControlledObject = ControlledObjects[i],
                Slots = new WeedDryerSlot[slotsPerObject]
            };

            for (int k = 0; k < slotsPerObject; k++)
            {
                _slots[j] = new WeedDryerSlot {ControlledObject = _controlledObjects[i]};

                _controlledObjects[i].Slots[k] = _slots[j];

                j++;
            }
        }
    }

    void Update()
    {
        for(int i = 0; i < _slots.Length; i++)
        {
            if(!_slots[i].IsEmpty)
            {
                if (_slots[i].TimeDryed < DryingTime)
                    _slots[i].TimeDryed += Time.deltaTime;
                else
                {
                    WeedDryed(_slots[i]);
                }
            }
        }
    }

    public void PutWeedAtSlot(WeedDryerSlot slot, float grams)
    {
        slot.Grams = grams;
        slot.ControlledObject.OnWeedEnter();
        slot.IsEmpty = false;

        // check if full now
        var attemptslot = _slots.FirstOrDefault(x => x.IsEmpty);
        if (attemptslot == null)
            _state = WeedDryerState.Full;
    }

    public bool PutWeed(float grams)
    {
        Debug.Log("Added plant to drying!");
        var firstempty = _slots.FirstOrDefault(x => x.IsEmpty);
        if (firstempty != null)
        {
            PutWeedAtSlot(firstempty, grams);
            return true;
        }
        return false; // the dryer is full!
    }

    private float WeedDryed(WeedDryerSlot slot)
    {
        float returnweed = 0f;

        if (Infrastructure != null)
        {
			bool success = Infrastructure.AddWeed((int)slot.Grams);

            if (success)
				// TODO: currently polling every frame when storages full, make it so that its event based ot polls less often
            {
                Debug.Log("Gained "+ (int)slot.Grams+"g weed!");
                slot.IsEmpty = true;
                slot.TimeDryed = 0f;
                returnweed = slot.Grams;
                slot.Grams = 0f;
                slot.ControlledObject.OnWeedLeave();
                _state = _slots.Any(x => !x.IsEmpty) ? WeedDryerState.NotEmptyNotFull : WeedDryerState.Empty;
            }
        }

        return returnweed;
    }

    public WeedDryerSave GetState()
    {
        WeedDryerSave state = new WeedDryerSave();
        for (int i = 0; i < Slots; i++)
        {
            state.Slots.Add(_slots[i]);
        }
        return state;
    }

    public void SetState(WeedDryerSave state)
    {
        Start(); // reset state the dirty way

        if(state.Slots.Count != _slots.Length)
        {
            Debug.LogError("Loaded weeddryer had different slot count!");
            return;
        }
        for(int i = 0; i < _slots.Length; i++)
        {
            if(!state.Slots[i].IsEmpty)
            {
                PutWeedAtSlot(_slots[i], state.Slots[i].Grams);
                _slots[i].TimeDryed = state.Slots[i].TimeDryed;
            }          
        }
    }
}

public class WeedDryerControlledObject
{
    public WeedDryerSlot[] Slots;
    public GameObject ControlledObject;

    public void OnWeedLeave()
    {
        bool remainActive = false;

        foreach (var weedDryerSlot in Slots)
        {
            remainActive = !weedDryerSlot.IsEmpty;
        }

        ControlledObject.SetActive(remainActive);
    }

    public void OnWeedEnter()
    {
        ControlledObject.SetActive(true);
    } 
}

[System.Serializable]
public class WeedDryerSlot
{
    public bool IsEmpty = true;
    public float TimeDryed = 0f;
    public float Grams;
    [System.NonSerialized]
    public WeedDryerControlledObject ControlledObject;
}

public enum WeedDryerState
{
    Empty,
    NotEmptyNotFull,
    Full
}

[System.Serializable]
public class WeedDryerSave
{
    public List<WeedDryerSlot> Slots = new List<WeedDryerSlot>();
}