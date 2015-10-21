using UnityEngine;
using System.Collections;

public class MeshParticleEngine : MonoBehaviour 
{
	public struct MeshData
	{
		public Vector3 Position;
		public Vector4 Color;
	}

	public struct MeshPhysicsData
	{
		public Vector3 Velocity;
	}

	const int kNumThreadsX = 256;

	[SerializeField]
	Mesh _mesh;

	Material _material;

	// Compute
	private const string kInitKernel = "InitKernel";
	private const string kUpdateKernel = "UpdateKernel";

	ComputeShader _shaderCompute;
	ComputeBuffer _verticesBuffer;
	ComputeBuffer _particlesBuffer;
	ComputeBuffer _countArgsBuffer;
	ComputeBuffer _particlesPhysicsBuffer;

	ComputeBuffer _VP;
	ComputeBuffer _I_VP;

	int _initKernelId;
	int _updateKernelId;

	int _numThreadGroupsX;

	int _instancesCount;

	void Start()
	{
		CreateAssets();
		DoInit();
	}

	void CreateAssets()
	{
		_material = Resources.Load<Material>("ParticleInstancingMat");

		_shaderCompute = Resources.Load<ComputeShader>("ParticleMeshCompute");

		_initKernelId = _shaderCompute.FindKernel(kInitKernel);
		_updateKernelId = _shaderCompute.FindKernel(kUpdateKernel);

		_instancesCount = _mesh.vertices.Length/3;
		_numThreadGroupsX = Mathf.CeilToInt(((float)_instancesCount) / ((float)kNumThreadsX));
		Debug.Log("_numThreadGroupsX " + _numThreadGroupsX);
	}

	void DoInit()
	{
		if(_verticesBuffer != null)
		{
			_verticesBuffer.Release();
			_verticesBuffer = null;
		}

		_verticesBuffer = new ComputeBuffer(_mesh.vertices.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(MeshParticleEngine.MeshData)) );
		_particlesBuffer = new ComputeBuffer(_instancesCount*100, System.Runtime.InteropServices.Marshal.SizeOf(typeof(MeshParticleEngine.MeshData)), ComputeBufferType.Append );
		_countArgsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
		_particlesPhysicsBuffer = new ComputeBuffer(_instancesCount*100, System.Runtime.InteropServices.Marshal.SizeOf(typeof(MeshParticleEngine.MeshPhysicsData)), ComputeBufferType.Append );

		_VP = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4)) );
		_I_VP = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4)) );

		_shaderCompute.SetBuffer(_initKernelId, "_VerticesBuffer", _verticesBuffer);
		_shaderCompute.SetBuffer(_initKernelId, "_ParticlesBuffer", _particlesBuffer);
		_shaderCompute.SetBuffer(_initKernelId, "_ParticlesPhysicsBuffer", _particlesPhysicsBuffer);

		_shaderCompute.SetBuffer(_initKernelId, "_VP", _VP);
		_shaderCompute.SetBuffer(_initKernelId, "_I_VP", _I_VP);

		Matrix4x4 VP = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
		Matrix4x4 I_VP = Matrix4x4.Inverse(VP);
		_VP.SetData(new Matrix4x4 [] { VP });
		_I_VP.SetData(new Matrix4x4 [] { I_VP });

		_shaderCompute.SetVector("_ParticlesSize", new Vector4(_instancesCount, 0f, 0f, 0f));
		Debug.Log("ParticlesSizes.x " + _instancesCount);
		_material.SetBuffer("_ParticlesBuffer", _particlesBuffer);

		DoInitMeshBuffer();

		_shaderCompute.Dispatch(_initKernelId, _numThreadGroupsX, 1, 1);
	}

	void DoInitMeshBuffer()
	{
		MeshData [] vertices = new MeshData[_mesh.vertices.Length];
		for (int i = 0; i < _mesh.vertices.Length; ++i) 
		{
			vertices[i].Position = _mesh.vertices[i];
//			vertices[i].Color = _mesh.colors[i];
		}
		_verticesBuffer.SetData(vertices);
	}

	void Update()
	{
//		_shaderCompute.SetFloat("_Time", Time.time);
//		_shaderCompute.SetFloat("_DeltaTime", Time.deltaTime);
//
//		if(Input.GetMouseButtonUp(0))
//		{
//			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//			//Vector3 pos = ray.origin + ray.direction * 10f;
//			_shaderCompute.SetVector("_Explosion", new Vector4(pos.x, pos.y, pos.z, 1));
//		}
//		else
//		{
//			_shaderCompute.SetVector("_Explosion", Vector4.zero);
//		}
//
//		_shaderCompute.Dispatch(_updateKernel, _numThreadGroupsX, 1, 1);
	}

	void OnRenderObject()
	{
		_material.SetPass(0);
		int [] args = new int[]{0, 1, 0, 0};
		_countArgsBuffer.SetData(args);
		ComputeBuffer.CopyCount(_particlesBuffer, _countArgsBuffer, 0);
		_countArgsBuffer.GetData(args);
		//Debug.Log("Args[0] " + args[0] + ", Args[0] " + args[1] + ", Args[2] " + args[2] + ", Args[3] " + args[3]);
		//Graphics.DrawProceduralIndirect(MeshTopology.Points, _countArgsBuffer);
		Graphics.DrawProcedural(MeshTopology.Points, 1, 292);
	}
}
