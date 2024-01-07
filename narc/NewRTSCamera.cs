// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class NewRTSCamera : MonoBehaviour {

    Camera _cam;

    Vector3 _lookAt;

    public bool Disabled = false;

    public float zoomSpeed = 5f;
    public float moveSpeed = 5f;
    public float maxAngle = 70f;
    public float minAngle = 10f;

	void Start () {
        _cam = GetComponent<Camera>();
	}

    void To2DUnitVector(ref Vector3 vec)
    {
        vec.y = 0f;
        vec.Normalize();
    }

    void AddMovement(ref Vector3 movement)
    {
        // TODO: use getaxis?
        Vector3 deltaM = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            var forw = _cam.transform.forward;
            To2DUnitVector(ref forw);
            deltaM += forw;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            var forw = _cam.transform.forward;
            To2DUnitVector(ref forw);
            deltaM -= forw;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            var forw = -_cam.transform.right;
            To2DUnitVector(ref forw);
            deltaM += forw;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            var forw = _cam.transform.right;
            To2DUnitVector(ref forw);
            deltaM += forw;
        }
        movement += deltaM * GameTime.RealDeltaTime * moveSpeed;
    }

    void AddRotation(ref Vector3 movement)
    {
        if (Input.GetMouseButton(2))
        {
            Quaternion oldRot = _cam.transform.rotation;

            var rotHor = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * 5f, Vector3.up);
            var rotVert = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * 5f, -_cam.transform.right);
            Vector3 vec = _cam.transform.position - _lookAt;
            vec = rotHor * rotVert * vec;

            Vector3 result = _lookAt + vec;
            result = result - _cam.transform.position;

            // TODO: this is not good?
            Vector3 orig = _cam.transform.position;
            _cam.transform.position += result;
            _cam.transform.LookAt(_lookAt);
            _cam.transform.position = orig;

            Vector3 euler = _cam.transform.rotation.eulerAngles;
            if (_cam.transform.rotation.eulerAngles.x > maxAngle || _cam.transform.rotation.eulerAngles.x < minAngle)
            {
                euler.x = Mathf.Clamp(euler.x, minAngle, maxAngle);
                _cam.transform.rotation = Quaternion.Euler(euler);
                result.y = 0f;
            }
            movement += result;
        }
    }

    void AddZoom(ref Vector3 movement)
    {
        var mWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mWheel) > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 vec = _lookAt - _cam.transform.position;
            vec.Normalize();
            movement += zoomSpeed * vec * mWheel;
        }
    }
	
	void Update () {

        if (!Disabled)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, _cam.transform.forward, out hit, 20f))
                _lookAt = hit.point;
            else
                _lookAt = _cam.transform.position + _cam.transform.forward * 20;
            Vector3 movement = Vector3.zero;
            AddMovement(ref movement);
            AddRotation(ref movement);
            AddZoom(ref movement);
            if (!Physics.Linecast(_cam.transform.position, _cam.transform.position + movement))
            {
                _cam.transform.position += movement;
            }
        }
    }
}
