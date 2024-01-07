// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RangeGridEffect))]
[RequireComponent(typeof(PlaceableObject))]
public class PlantLight : MonoBehaviour {

    RangeGridEffect _effect;
    PlaceableObject _plObject;

    public int Power = 200;
    public Light Light;

	void Start () {
        _effect = GetComponent<RangeGridEffect>();
        _plObject = GetComponent<PlaceableObject>();

        if (Light == null)
        {
            Debug.LogError("PlantLight didn't have Light set!");
            return;
        }

        Light.enabled = false;

        if (_plObject.Infrastructure.AddPowerConsumer(this,Power))
        {
            _effect.EnableEffect();
            Light.enabled = true;
        }
        else
        {
            GameTime.OnMinute += TryEnable;
        }
	}

    void TryEnable()
    {
        if (_plObject.Infrastructure.AddPowerConsumer(this, Power))
        {
            _effect.EnableEffect();
            Light.enabled = true;
            GameTime.OnMinute -= TryEnable;
        }
    }

    void OnDestroy()
    {
        //_effect.DisableEffect();
        if(Light.enabled)
        {
            Debug.Log("Removed power consumer");
            _plObject.Infrastructure.RemovePowerConsumer(this);
        }
        else
        {
            Debug.Log("Light wasn't enabled!");
        }
    }
}
