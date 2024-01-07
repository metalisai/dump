// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class FadeDistance : MonoBehaviour {

    public float Distance = 50f;
    public GameObject DistanceTo;
    public Housing housing;
    MeshRenderer _renderer;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        //_meshf = GetComponent<MeshFilter>();
        if (DistanceTo == null)
            DistanceTo = gameObject;
    }

	void Update () {

        float dist = Vector3.Distance(Camera.main.transform.position, DistanceTo.transform.position);
        // show
        if (dist > Distance && _renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.On)
        {
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        // hide
        else if(dist <= Distance && _renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly 
            && housing != null && housing.Owner != null)
        {
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
	}
}
