using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GrassEngine : MonoBehaviour
{
	private string kInitGrassKernel = "InitGrassKernel";
	private string kUpdateGrassKernel = "UpdateGrassKernel";

	private int kThreadsX = 16;
	private int kThreadsY = 16;

	private int kMaxObstacles = 128;

	public struct GrassData
	{
		public Vector3 Position;
		public Vector3 Front;
		public Vector3 Right;
		public float Flattening;
		public Vector3 ExpansiveForce;
	}

	public struct ObstacleData
	{
		public Vector3 Position;
		public float Radius;
		public float ExpansiveForce;
	}

	[SerializeField]
	int _numGroupGrassX = 16;

	[SerializeField]
	int _numGroupGrassY = 16;

	ComputeBuffer _grassBuffer;
	public ComputeShader _grassComputeShader;

	ComputeBuffer _obstaclesBuffer;

	public Shader _grassShader;
	public Material _grassMaterial;

	public Texture _noiseTex;

	int _initGrassKernelId;
	int _updateGrassKernelId;

	int _numGrassItems;

	bool _isInit = false;

	void Start()
	{
		Init();
	}

	public void Init()
	{
		_grassShader = Resources.Load<Shader>("Shaders/GrassGeneratorShader");
		_grassMaterial = Resources.Load<Material>("GrassMat");
		_noiseTex = Resources.Load<Texture>("Noise");
		if(_noiseTex == null)
		{
			Debug.LogError("Not found noise");
		}

		_grassComputeShader = Resources.Load<ComputeShader>("ComputeShaders/GrassComputeShader");
		_initGrassKernelId = _grassComputeShader.FindKernel(kInitGrassKernel);
		_updateGrassKernelId = _grassComputeShader.FindKernel(kUpdateGrassKernel);

		_numGrassItems = _numGroupGrassX*_numGroupGrassY*kThreadsX*kThreadsY;
		_grassBuffer = new ComputeBuffer(_numGrassItems, System.Runtime.InteropServices.Marshal.SizeOf(typeof(GrassData)));
		_obstaclesBuffer = new ComputeBuffer(kMaxObstacles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(ObstacleData)));

		_grassComputeShader.SetFloat("_Width", _numGroupGrassX*kThreadsX);
		_grassComputeShader.SetFloat("_Height", _numGroupGrassY*kThreadsY);
		_grassComputeShader.SetTexture(_initGrassKernelId, "_NoiseTex", _noiseTex);

		_grassMaterial.SetTexture("_NoiseTex", _noiseTex);
		_grassMaterial.SetFloat("_Width", _numGroupGrassX*kThreadsX);
		_grassMaterial.SetFloat("_Height", _numGroupGrassY*kThreadsY);

		_grassComputeShader.SetBuffer(_initGrassKernelId, "_GrassBuffer", _grassBuffer);
		_grassComputeShader.SetBuffer(_updateGrassKernelId, "_GrassBuffer", _grassBuffer);
		_grassComputeShader.SetBuffer(_updateGrassKernelId, "_ObstaclesBuffer", _obstaclesBuffer);
		_grassComputeShader.SetInt("_NumObstacles", 0);
		_grassMaterial.SetBuffer("_GrassBuffer", _grassBuffer);

		_grassComputeShader.Dispatch(_initGrassKernelId, _numGroupGrassX, _numGroupGrassY, 1);

		_isInit = true;
	}

	public void SetObstaclesData(ObstacleData[] obstaclesData)
	{
		_grassComputeShader.SetFloat("_DeltaTime", Time.deltaTime);

		_obstaclesBuffer.SetData(obstaclesData);
		_grassComputeShader.SetInt("_NumObstacles", obstaclesData.Length);
	}

	void Update()
	{
		_grassComputeShader.Dispatch(_updateGrassKernelId, _numGroupGrassX, _numGroupGrassY, 1);
	}

	void OnRenderObject()
	{
		if(!_isInit)
		{
			return;
		}

		_grassMaterial.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Points, 1, _numGrassItems);
	}

	void OnDestroy()
	{
		// Unity cry if the GPU buffer isn't manually cleaned
		_grassBuffer.Release();
	}
}
