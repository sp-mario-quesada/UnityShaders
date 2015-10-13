using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeMapGenerator : MonoBehaviour 
{
	[SerializeField]
	List<GameObject> _targets = new List<GameObject>();

	[SerializeField]
	string _cubeShaderPropertyName = "_Cube";

	[SerializeField]
	bool _renderEveryFrame = false;

	[SerializeField]
	Camera _camera;

	[SerializeField]
	Cubemap _cube;

	[SerializeField]
	int _cubeResolution = 512;

	[SerializeField]
	LayerMask _cullingMask;

	Skybox _skybox;

	void Awake()
	{
		Init();
	}

	void CreateAssets(int cullingMask)
	{
		if(_cube == null)
		{
			_cube = new Cubemap(_cubeResolution, TextureFormat.ARGB32, false);
		}

		if(_camera == null)
		{
			GameObject cameraGo = new GameObject("ReflectionCamera");
			cameraGo.transform.SetParent(transform);
			_camera = cameraGo.AddComponent<Camera>();
		}

		_camera.cullingMask = cullingMask;
		_camera.fieldOfView = 45f;
		_camera.clearFlags = CameraClearFlags.Skybox;
		_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		_camera.gameObject.hideFlags = HideFlags.HideAndDontSave;
		_camera.enabled = false;
	}

	void Init()
	{
		CreateAssets(_cullingMask.value);

		RenderToCubemap();

	}

	//void OnRenderObject()
	void OnPostRender()
	{
//		if(Camera.current.name == "ReflectionCamera")
//		{
//			return;
//		}

//		if(_renderEveryFrame)
//		{
//			RenderToCubemap();
//
//			for (int i = 0; i < _targets.Count; ++i) 
//			{
//				Renderer renderer = _targets[i].GetComponentInChildren<Renderer>();
//				renderer.sharedMaterial.SetTexture(_cubeShaderPropertyName, _cube);
//			}
//		}
	}

	void RenderToCubemap()
	{
		_camera.RenderToCubemap(_cube);
	}
}
