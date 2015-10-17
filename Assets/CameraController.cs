using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour 
{
	[SerializeField]
	int _mouseButtonToChange = 0;

	[SerializeField]
	float _cameraMoveSpeed = 40;

	[SerializeField]
	float _lerpSpeed = 2f;

	public List<Camera> _cameras;
	int _currentCameraIdx = 0;

	Vector3 _targetPosition;

	public Camera CurrentCamera
	{
		get
		{
			return _cameras[_currentCameraIdx];
		}
	}

	void Awake()
	{
		_currentCameraIdx = 0;

		_cameras = new List<Camera>(GetComponentsInChildren<Camera>());

		for (int i = 0; i < _cameras.Count; ++i) 
		{
			if(i != 0)
			{
				_cameras[i].gameObject.SetActive(false);
			}
		}

		if(!_cameras[0].gameObject.activeSelf)
		{
			_cameras[0].gameObject.SetActive(true);
		}

		_targetPosition = CurrentCamera.transform.position;
	}

	void DisableAllCameras()
	{
		for (int i = 0; i < _cameras.Count; ++i) 
		{
			_cameras[i].gameObject.SetActive(false);
		}
	}

	void Update()
	{
		if(Input.GetMouseButtonUp(_mouseButtonToChange))
		{
			DisableAllCameras();

			_currentCameraIdx = (++_currentCameraIdx)%_cameras.Count;
			_cameras[_currentCameraIdx].gameObject.SetActive(true);

			_targetPosition = CurrentCamera.transform.position;
		}

		float h =Input.GetAxis("Horizontal");
		float v =Input.GetAxis("Vertical");

		Vector3 forward = CurrentCamera.transform.forward;
		forward.y = 0;

		Vector3 right = CurrentCamera.transform.right;
		right.y = 0;

		_targetPosition += forward * v * _cameraMoveSpeed * Time.deltaTime;
		_targetPosition += right * h * _cameraMoveSpeed * Time.deltaTime;

		CurrentCamera.transform.position = Vector3.Lerp(CurrentCamera.transform.position, _targetPosition, Time.deltaTime * _lerpSpeed);
	}
}
