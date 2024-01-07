// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class Storage : MonoBehaviour, ISaveable
{

    public int Capacity;
    public StorageType StorageType;

    public GameObject[] ControlledObjects;
    public MeshRenderer[] ControlledObjectRenderers;

    public int AmountStored { get{ return _amountStored;} }

    public Color GhostColor = Color.white;

    public StorageState State {
        get
        {
            return _state;
        }
        set
        {
            // was empty and will not be empty
            if (_state == StorageState.Empty && value != StorageState.Empty)
            {
                for (int i = 0; i < ControlledObjects.Length; i++)
                {
                    SetBlockState(i, i < _objectsActivated ? BlockState.Enabled : BlockState.Disabled);
                }
            }
            // was not empty and will be empty
            else if (_state != StorageState.Empty && value == StorageState.Empty)
            {
                for (int i = 0; i < ControlledObjects.Length; i++)
                {
                    SetBlockState(i, BlockState.Transparent);
                }
            }
            _state = value;
        }
    }

    public enum BlockState
    {
        Disabled,
        Transparent,
        Enabled
    }

    private int _amountStored = 0;

    //private int _amountPerObject;
    private int _objectsActivated=0;

    private StorageState _state = StorageState.Empty;

    void Reset()
    {
        AddProduct(-_amountStored);
        _objectsActivated = 0;
    }

    void Awake()
    {
        if(ControlledObjects.Length == 0)
            Debug.LogError("Assign Controlled objects to storage!");

        //_amountPerObject = Capacity/ControlledObjects.Length;

        ControlledObjectRenderers = new MeshRenderer[ControlledObjects.Length];

        for(int i = 0; i < ControlledObjects.Length; i++)
        {
            ControlledObjectRenderers[i] = ControlledObjects[i].GetComponent<MeshRenderer>();
            SetBlockState(i, BlockState.Transparent);
        }
    }



    public bool AddProduct(int amount)
    {
        bool addingSuccessful = false;

        if (_amountStored + amount <= Capacity && _amountStored+amount >= 0)
        {
            _amountStored += amount;
            UpdateStorageObjects();

            State = StorageState.NotEmptyNotFull;

            addingSuccessful = true;
        }

        if (_amountStored == Capacity)
            State = StorageState.Full;
        else if (_amountStored == 0)
            State = StorageState.Empty;
        return addingSuccessful; // not enough room in storage / already empty
    }

    private void UpdateStorageObjects()
    {
        int afterAddActivated = Mathf.CeilToInt((float)(_amountStored)/* / _amountPerObject*/);

        ActivateObjects(afterAddActivated - _objectsActivated);
    }

    private void ActivateObjects(int numObjects)
    {
        if (numObjects >= 0)
        {
            for (int i = 0; i < numObjects; i++)
            {
                SetBlockState(_objectsActivated, BlockState.Enabled);
                _objectsActivated++;
            }
        }
        else
        {
            for (int i = 0; i > numObjects; i--)
            {
                _objectsActivated--;
                SetBlockState(_objectsActivated, BlockState.Disabled);
            }
        }
    }

    private void SetBlockState(int index, BlockState state)
    {
        switch (state)
        {
            case BlockState.Enabled:
                ControlledObjects[index].SetActive(true);
                ControlledObjectRenderers[index].material.color = Color.white;
                ControlledObjectRenderers[index].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                break;
            case BlockState.Transparent:
                ControlledObjects[index].SetActive(true);
                ControlledObjectRenderers[index].material.color = GhostColor;
                ControlledObjectRenderers[index].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                break;
            case BlockState.Disabled:
                ControlledObjects[index].SetActive(false);
                break;
            default:
                Debug.LogError("Invalid argument!");
                break;
        }
    }

    public void SetState(System.Object state)
    {
        var cstate = state as StorageSave;
        Reset();
        AddProduct(cstate.AmountStored);
    }

    public System.Object GetState()
    {
        StorageSave state = new StorageSave {AmountStored = AmountStored};
        return state;
    }
}

[System.Serializable]
public class StorageSave
{
    public int AmountStored;
}


public enum StorageType
{
    WeedStorage
}
public enum StorageState
{
    Empty,
    NotEmptyNotFull,
    Full
}