// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LightController : MonoBehaviour {

    public List<GameObject> Lights;
    public int TurnOnHour = 19;
    public int TurnOffHour = 5;

    private LCLight[] CachedLights;
    private bool stateLastTick = false;

    struct LCLight
    {
        public GameObject EmissionObject;
        public Light Light;
    }

	void Start () {
        CachedLights = new LCLight[Lights.Count];

        for (int i = 0; i < Lights.Count; i++)
        {
            CachedLights[i].EmissionObject = Lights[i].transform.FindChild("Light_ON").gameObject;
            CachedLights[i].Light = GetComponentInChildren<Light>();

            CachedLights[i].EmissionObject.SetActive(false);
        }
	}
	
	void Update () {
        bool stateThisFrame = GameTime.Hour >= TurnOnHour || GameTime.Hour < TurnOffHour;
        if(stateThisFrame != stateLastTick)
        {
            foreach(var light in CachedLights)
            {
                if (light.EmissionObject != null)
                {
                    light.EmissionObject.SetActive(stateThisFrame);
                }
            }
        }
        stateLastTick = stateThisFrame;
	}
}