//#define GRASS_CPU

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

	GrassData[] _grassDataTestCPU;
	ObstacleData[] _obstaclesDataTestCPU = null;

	int _numGrassItems;

	bool _isInit = false;

	void Start()
	{
		Init();
	}

	public void Init()
	{
		_grassShader = Resources.Load<Shader>("Shaders/GrassGeneratorShader");
		_grassMaterial = Resources.Load<Material>("GrassGeneratorMat");
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
#if GRASS_CPU
		_grassDataTestCPU = new GrassData[_numGrassItems];
		_grassBuffer.GetData(_grassDataTestCPU);
#endif
		_isInit = true;
	}

	public void SetObstaclesData(ObstacleData[] obstaclesData)
	{
		_grassComputeShader.SetFloat("_DeltaTime", Time.deltaTime);

		_obstaclesBuffer.SetData(obstaclesData);
		_grassComputeShader.SetInt("_NumObstacles", obstaclesData.Length);
#if GRASS_CPU
		_obstaclesDataTestCPU = obstaclesData;
#endif
	}

	void Update()
	{
#if GRASS_CPU
		DoUpdateInCPU();
#else
		_grassComputeShader.Dispatch(_updateGrassKernelId, _numGroupGrassX, _numGroupGrassY, 1);
#endif
	}

	void DoUpdateInCPU()
	{
		for (int idx = 0; idx < _grassDataTestCPU.Length; ++idx) 
		{
			DoUpdateGrassKernelCPU(ref _grassDataTestCPU, idx);
		}

		_grassBuffer.SetData(_grassDataTestCPU);
	}

	void DoUpdateGrassKernelCPU (ref GrassData[] datas, int idx)
	{
		GrassData data = datas[idx];
		Vector3 expansiveForce = Vector3.zero;
		for(int i = 0; _obstaclesDataTestCPU != null && i < _obstaclesDataTestCPU.Length; ++i)
		{
			Vector3 dirToObstacle = _obstaclesDataTestCPU[i].Position - data.Position;
			float obstacleRadiusSQ = _obstaclesDataTestCPU[i].Radius*_obstaclesDataTestCPU[i].Radius;
			
			float distToObstacleSQ = dirToObstacle.x*dirToObstacle.x + dirToObstacle.y*dirToObstacle.y + dirToObstacle.z*dirToObstacle.z;
			if(distToObstacleSQ-obstacleRadiusSQ < 0)
			{
				float flattening = (1f-smoothstep(0f, 1f, distToObstacleSQ)) * Time.deltaTime * 4f;
				data.Flattening = Mathf.Max(data.Flattening - flattening, 0.2f);
			}
			
			if(distToObstacleSQ-obstacleRadiusSQ < 4f)
			{
				float forceIntensity = 1-smoothstep(0f, 4f, distToObstacleSQ);
				expansiveForce += Vector3.Normalize(-dirToObstacle) * _obstaclesDataTestCPU[i].ExpansiveForce * forceIntensity;
				expansiveForce.y = 0f;
				data.ExpansiveForce = expansiveForce;
			}
		}
		datas[idx] = data;
	}

	float smoothstep(float a, float b, float val)
	{
		return Mathf.Clamp01((val-a)/(b-a));
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
