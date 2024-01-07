using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System;

public class Player
{
    public enum Mode
    {
        Interacting,
        Placing
    }

    Mode currentMode = Mode.Interacting;

    [System.Serializable]
    public struct PlayerSettings
    {
        public float CameraMovementSpeed;
        public float CameraZoomSpeed;
    }
    Camera camera;

    float cameraMovementSpeed;
    //float cameraZoomSpeed;

    InteractionHandler _interactions = new InteractionHandler();
    public ItemPlacer Placer = new ItemPlacer();

    public Player(Camera camera, ref PlayerSettings settings)
    {
        this.camera = camera;
        cameraMovementSpeed = settings.CameraMovementSpeed*0.05f;
        //cameraZoomSpeed = settings.CameraZoomSpeed;

        ItemPlacer.PlacerStartEventHandler startPlace = () =>
        {
            currentMode = Mode.Placing;
        };

        ItemPlacer.PlacerStartEventHandler onEndPlace = () =>
        {
            currentMode = Mode.Interacting;
        };
        Placer.PlacingStarted += startPlace;
        Placer.PlacingEnded += onEndPlace;

		_targetPosition = camera.transform.position;
        _interactions.Init();
    }

	public void SetCameraPosition(Vector3 position)
	{
		_targetPosition = position;
	}

	public void SetCameraRotation(Quaternion rotation)
	{
		
	}

    public void Update()
    {
        switch(currentMode)
        {
            case Mode.Interacting:
                _interactions.Update();
                break;
            case Mode.Placing:
                Placer.Update();
                break;
        }
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
            var forw = camera.transform.forward;
            To2DUnitVector(ref forw);
            deltaM += forw;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            var forw = camera.transform.forward;
            To2DUnitVector(ref forw);
            deltaM -= forw;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            var forw = -camera.transform.right;
            To2DUnitVector(ref forw);
            deltaM += forw;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            var forw = camera.transform.right;
            To2DUnitVector(ref forw);
            deltaM += forw;
        }
        movement += deltaM * Time.unscaledDeltaTime * cameraMovementSpeed * (_lookAt-camera.transform.position).magnitude;
    }

    Vector3 _lookAt;
	Vector3 _targetPosition;
	Quaternion _targetRotation;
    public float zoomSpeed = 1f;
    public float moveSpeed = 5f;
    public float maxAngle = 70f;
    public float minAngle = 50f;

    void AddRotation(ref Vector3 movement)
    {
        float horMove = 0;
        float vertMove = 0;

        if(Input.GetMouseButton(2))
        {
            horMove += Input.GetAxis("Mouse X") * 5f;
            vertMove += Input.GetAxis("Mouse Y") * 5f;
        }

        if (Input.GetKey(KeyCode.E))
            horMove += 25f * Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.Q))
            horMove -= 25f * Time.unscaledDeltaTime;

        if (Input.GetMouseButton(2) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q))
        {
			Vector3 targetRight = _targetRotation*Vector3.right;
			var rotHor = Quaternion.AngleAxis(horMove, Vector3.up);
			var rotVert = Quaternion.AngleAxis(vertMove, -targetRight);
			Vector3 offsetFromLookat = _targetPosition - _lookAt;
			offsetFromLookat = rotHor * rotVert * offsetFromLookat;
			Vector3 result = _lookAt + offsetFromLookat;
			_targetRotation = Quaternion.LookRotation ((_lookAt - result).normalized);

			Vector3 euler = _targetRotation.eulerAngles;
			if (_targetRotation.eulerAngles.x > maxAngle || _targetRotation.eulerAngles.x < minAngle)
            {
                euler.x = Mathf.Clamp(euler.x, minAngle, maxAngle);
                //camera.transform.rotation = Quaternion.Euler(euler);
				_targetRotation = Quaternion.Euler (euler);
                result.y = 0f;
            }
			movement += result-_targetPosition;
        }
    }

    void AddZoom(ref Vector3 movement)
    {
        var mWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mWheel) > 0/* && !EventSystem.current.IsPointerOverGameObject()*/)
        {
            Vector3 vec = _lookAt - camera.transform.position;
            vec.Normalize();
            movement += zoomSpeed * vec * mWheel * (_lookAt-camera.transform.position).magnitude;
        }
    }

    public void LateUpdate()
    {
        RaycastHit hit;
		if (Physics.Raycast(_targetPosition, _targetRotation*Vector3.forward, out hit, 500f))
            _lookAt = hit.point;
        else
            _lookAt = camera.transform.position + camera.transform.forward * 20;
        Vector3 movement = Vector3.zero;
        AddMovement(ref movement);
        AddRotation(ref movement);
        AddZoom(ref movement);
		if (!Physics.Linecast(_targetPosition, _targetPosition + movement))
        {
			_targetPosition += movement;
        }

		Vector3 lerpedPosition = Vector3.Lerp(camera.transform.position, _targetPosition, Time.unscaledDeltaTime * 10.0f);
		camera.transform.position = lerpedPosition;
		camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, _targetRotation, Time.unscaledDeltaTime*10.0f);
    }
}
