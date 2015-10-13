using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
//[ExecuteInEditMode]
public class CameraDepthEnabler : MonoBehaviour 
{
	Camera _camera;

	void Start()
	{
		_camera = GetComponent<Camera>();
		_camera.depthTextureMode = DepthTextureMode.DepthNormals;
	}
}
