using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour 
{
	public List<Camera> _cameras;
	int _currentCameraIdx = 0;

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
		if(Input.GetMouseButtonUp(0))
		{
			DisableAllCameras();

			_cameras[(++_currentCameraIdx)%_cameras.Count].gameObject.SetActive(true);
		}
	}
}
