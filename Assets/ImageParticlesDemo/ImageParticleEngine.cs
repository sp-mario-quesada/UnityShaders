using UnityEngine;
using System.Collections;

public class ImageParticleEngine : MonoBehaviour 
{
	public struct ParticleData
	{
		public Vector3 Position;
		public Vector4 Color;
	}

	public struct ParticlePhysicsData
	{
		public Vector3 Velocity;
	}

	const int kNumThreadsX = 16;
	const int kNumThreadsY = 16;
	const int kNumThreadsZ = 1;

	[SerializeField]
	Texture _image;

	//[SerializeField]
	int _imageLayers = 1;

	Material _material;

	// Compute
	private const string kInitParticlesKernel = "InitParticlesKernel";
	private const string kUpdateParticlesKernel = "UpdateParticlesKernel";

	ComputeShader _shaderCompute;
	ComputeBuffer _particlesBuffer;
	ComputeBuffer _particlesPhysicsBuffer;
	int _initImageKernelId;
	int _updateParticlesKernel;

	int _numThreadGroupsX;
	int _numThreadGroupsY;
	int _numThreadGroupsZ;

	int _totalNumParticles;

	void Start()
	{
		CreateAssets();
		DoInit();
	}

	void CreateAssets()
	{
		_material = Resources.Load<Material>("ParticleImageMat");

		_shaderCompute = Resources.Load<ComputeShader>("ParticleImageCompute");
		_initImageKernelId = _shaderCompute.FindKernel(kInitParticlesKernel);
		_updateParticlesKernel = _shaderCompute.FindKernel(kUpdateParticlesKernel);

		_numThreadGroupsX = Mathf.CeilToInt(_image.width / kNumThreadsX);
		_numThreadGroupsY = Mathf.CeilToInt(_image.height / kNumThreadsY);
		_numThreadGroupsZ = Mathf.Max(Mathf.CeilToInt(_imageLayers / kNumThreadsX), 1);

		_totalNumParticles = _image.width * _image.height * _imageLayers;
	}

	void DoInit()
	{
		if(_particlesBuffer != null)
		{
			_particlesBuffer.Release();
			_particlesBuffer = null;
		}

		_particlesBuffer = new ComputeBuffer(_totalNumParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(ImageParticleEngine.ParticleData)) );
		_particlesPhysicsBuffer = new ComputeBuffer(_totalNumParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(ImageParticleEngine.ParticlePhysicsData)) );

		_shaderCompute.SetBuffer(_initImageKernelId, "_ParticlesBuffer", _particlesBuffer);
		_shaderCompute.SetBuffer(_initImageKernelId, "_ParticlesPhysicsBuffer", _particlesPhysicsBuffer);

		_shaderCompute.SetBuffer(_updateParticlesKernel, "_ParticlesBuffer", _particlesBuffer);
		_shaderCompute.SetBuffer(_updateParticlesKernel, "_ParticlesPhysicsBuffer", _particlesPhysicsBuffer);

		_shaderCompute.SetVector("_ImageSize", new Vector4(_image.width, _image.height, 0f, 0f));
		_shaderCompute.SetTexture(_initImageKernelId, "_Image", _image);
		_shaderCompute.SetVector("_Position", transform.position);
		_shaderCompute.SetVector("_ParticleHalfSize", new Vector4(1f, 1f, 1f, 1f)*0.016f);
		_shaderCompute.SetInt("_ImageLayers", _imageLayers);

		_material.SetBuffer("_ParticlesBuffer", _particlesBuffer);
		_material.SetVector("_ImageSize", new Vector4(_image.width, _image.height, 0f, 0f));
		_material.SetVector("_ParticleHalfSize", new Vector4(1f, 1f, 0f, 1f)*0.016f);

		_shaderCompute.Dispatch(_initImageKernelId, _numThreadGroupsX, _numThreadGroupsY, _numThreadGroupsZ);
	}

	void Update()
	{
		_shaderCompute.SetFloat("_Time", Time.time);
		_shaderCompute.SetFloat("_DeltaTime", Time.deltaTime);

		if(Input.GetMouseButton(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			//Vector3 pos = ray.origin + ray.direction * 10f;
			_shaderCompute.SetVector("_Explosion", new Vector4(pos.x, pos.y, pos.z, 1));
		}
		else
		{
			_shaderCompute.SetVector("_Explosion", Vector4.zero);
		}

		_shaderCompute.Dispatch(_updateParticlesKernel, _numThreadGroupsX, _numThreadGroupsY, _numThreadGroupsZ);
	}

	void OnRenderObject()
	{
		_material.SetVector("_WorldSpaceCameraRight", Camera.main.transform.right);
		_material.SetVector("_WorldSpaceCameraUp", Camera.main.transform.up);
		_material.SetPass(1);
		Graphics.DrawProcedural(MeshTopology.Points, 1, _totalNumParticles);
	}
}
