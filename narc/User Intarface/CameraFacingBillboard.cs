// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
    private Camera _mCamera;

    void Start()
    {
        _mCamera = Camera.main;
    }

    void Update()
    {
        transform.LookAt(transform.position + _mCamera.transform.rotation * Vector3.back,
            _mCamera.transform.rotation * Vector3.up);
    }
}