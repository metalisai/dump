using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ObjectLogic(ActiveObject.ObjectTag.WoodGenerator)]
public class WoodGenerator : ElectricDevice, IResourceStore, PowerProvider
{
    float internalCapacitor = 0;
    float powerPerSecond = 50;
    const int maxStoredWood = 20;
    const float secondsPerWood = 20.0f;

    int storedWood = 0;
    float burnProgress = 0.0f;
    bool haveFuel = false;

    const float _animationAcceleration = 0.1f;
    float _animationSpeed = 0.0f;
    Animator _animator;

    public bool IsGenericStorage { get { return false; } }

    public GameObject rsgameObject { get { return gameObject; } }

    new void Awake()
    {
        GameTime.MediumTick += Tick;
        base.Awake();
    }

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _animator.enabled = true;
        deviceFlags |= DeviceFlags.Provider;
    }

    public void Tick(float tickDelta)
    {
        if (burnProgress <= 0.0f)
        {
            if (storedWood >= 1)
            {
                storedWood--;
                burnProgress = 1.0f;
            }
        }
        if (burnProgress > 0.0f)
        {
            haveFuel = true;
            burnProgress -= tickDelta / secondsPerWood;
        }
        else
            haveFuel = false;
        if (haveFuel)
        {
            internalCapacitor += tickDelta * powerPerSecond;
            internalCapacitor = Mathf.Min(internalCapacitor, 500.0f);
            //DebugUI.ShowMessage(tickDelta * powerPerSecond);
            _animationSpeed += _animationAcceleration * tickDelta;
        }
        else
        {
            _animationSpeed -= _animationAcceleration * tickDelta;
        }
        _animationSpeed = Mathf.Clamp01(_animationSpeed);
        _animator.speed = _animationSpeed;
    }

    public float GetElectricity(ElectricDevice requester, float amount, bool takeAnything)
    {
        if (!takeAnything)
        {
            if (internalCapacitor >= amount)
            {
                internalCapacitor -= amount;
                return amount;
            }
        }
        else
        {
            float ret = Mathf.Min(internalCapacitor, amount);
            internalCapacitor -= ret;
            return ret;
        }
        return 0;
    }

    new void OnDestroy()
    {
        GameTime.MediumTick -= Tick;
        base.OnDestroy();
    }

    public int GetResourceCount(ResourceType type)
    {
        if (type == ResourceType.Wood)
            return storedWood;
        return 0;
    }

    public bool GetConstraint(ResourceType rtype, out int value)
    {
        if (rtype == ResourceType.Wood)
        {
            value = maxStoredWood;
            return true;
        }
        value = 0;
        return false;
    }

    public int TakeResource(ResourceType type, int amount, bool allowLess = true)
    {
        if (type == ResourceType.Wood)
        {
            if (!allowLess && amount > storedWood)
                return 0;
            return Mathf.Min(amount, storedWood);
        }
        return 0;
    }

    public StoredResource[] GetStoredResources()
    {
        if(storedWood > 0)
        {
            var ret = new StoredResource[1];
            ret[0].resourceName = StoredResource.GetResourceName(ResourceType.Wood);
            ret[0].resourceAmount = storedWood;
            ret[0].resourceType = ResourceType.Wood;
            return ret;
        }
        return new StoredResource[0];
    }

    public StorageResourceConstraint[] GetConstraints()
    {
        var ret = new StorageResourceConstraint[1];
        ret[0].resourceName = StoredResource.GetResourceName(ResourceType.Wood);
        ret[0].resourceType = ResourceType.Wood;
        ret[0].resourceAmount = maxStoredWood;
        return ret;
    }

    public bool StoreResource(ResourcePack resource)
    {
        Debug.Assert(resource.ResourceType == ResourceType.Wood, "WoodGenerator can only store wood");
        if (resource.ResourceType == ResourceType.Wood)
        {
            storedWood += resource.Amount;
            return true;
        }
        else
            return false;
    }

    public int GetSpareResourceCount(ResourceType type)
    {
        return 0;
    }
}
