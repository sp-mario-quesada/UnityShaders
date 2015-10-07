using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GPUParticleSystem : MonoBehaviour
{
	struct Particle
	{
		public Vector3 Position;
		public Vector3 Velocity;
		public Vector3 Size;
	}

	const int kNumThreadsX = 16;
	const int kNumThreadsY = 16;

	const int kNumThreadsVerticesX = 4 * 6 * 6;
	const int kNumThreadsVerticesY =         6;

	const string kKernelMoveParticles = "KernelMoveParticles";
	const string kKernelResizeVertices = "KernelResizeVertices";
	const string kKernelMeltVertices = "KernelMeltVertices";

	[SerializeField]
	int _particleGroupsX = 6;

	[SerializeField]
	int _particleGroupsY = 6;

	int _numParticlesX;
	int _numParticlesY;
	int _numParticles;

	[SerializeField]
	Material _particleMaterial;

	// Compute Shader
	[SerializeField]
	ComputeShader _computeShader;
	int _kernelMoveParticles;
	int _kernelResizeVerticesId;
	int _kernelMeltParticlesId;

	List<GameObject> _particlesGo = new List<GameObject>();
	Particle[] _particlesData;
	ComputeBuffer _particlesBuffer;

	Vector3[] _verticesData;
	ComputeBuffer _verticesBuffer;
	int _currentCopiedVertices = 0;

	public void Init()
	{
		_numParticlesX = _particleGroupsX * kNumThreadsX;
		_numParticlesY = _particleGroupsY * kNumThreadsY;
		_numParticles = _numParticlesX * _numParticlesY;

		_currentCopiedVertices = 0;

		_particleMaterial = Resources.Load<Material>("GPUParticleMat");

		_computeShader = Resources.Load<ComputeShader>("GPUParticleUpdater");
		_kernelMoveParticles = _computeShader.FindKernel(kKernelMoveParticles);
		_kernelResizeVerticesId = _computeShader.FindKernel(kKernelResizeVertices);
		_kernelMeltParticlesId = _computeShader.FindKernel(kKernelMeltVertices);

		_particlesData = new Particle[_numParticles];
		_verticesData = new Vector3[4 * 6 * _numParticles];
		InitBuffer(_particlesData);

		for (int i = 0; i < _particlesData.Length; ++i) 
		{
			float id = ((float)i) / 10000.1f;
			CreateParticle(id);
		}

		// Set ComputeShader Parameters
		_particlesBuffer = new ComputeBuffer(_particlesData.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(GPUParticleSystem.Particle)));
		_particlesBuffer.SetData(_particlesData);

		_verticesBuffer = new ComputeBuffer(_verticesData.Length, 4*3);
		_verticesBuffer.SetData(_verticesData);

		_computeShader.SetBuffer(_kernelResizeVerticesId, "_Vertices", _verticesBuffer);
		_computeShader.SetBuffer(_kernelMoveParticles, "_Vertices", _verticesBuffer);
		_computeShader.SetBuffer(_kernelMeltParticlesId, "_Vertices", _verticesBuffer);

		_computeShader.SetBuffer(_kernelResizeVerticesId, "_ParticlesBuffer", _particlesBuffer);
		_computeShader.SetBuffer(_kernelMoveParticles, "_ParticlesBuffer", _particlesBuffer);
		_computeShader.SetBuffer(_kernelMeltParticlesId, "_ParticlesBuffer", _particlesBuffer);

		_computeShader.SetFloat("_Width", _numParticlesX);
		_computeShader.SetFloat("_Height", _numParticlesY);

		// Set Shader Parameters
		_particleMaterial.SetBuffer("_ParticlesBuffer", _particlesBuffer);
		_particleMaterial.SetBuffer("_Vertices", _verticesBuffer);
	}

	void InitBuffer(Particle[] particles)
	{
		Vector3 startPoint = new Vector3(-_numParticlesX/2, 0, -_numParticlesY/2) * 0.25f;
		for (int y = 0; y < _numParticlesY; ++y) 
		{
			for (int x = 0; x < _numParticlesX; ++x) 
			{
				int idx = x + _numParticlesX * y;

				particles[idx].Position = startPoint + new Vector3(x, 0, y);
				particles[idx].Velocity = Random.insideUnitSphere;
				particles[idx].Size = Vector3.one;
			}
		}
	}

	void Update()
	{
		_computeShader.SetFloat("_DeltaTime", Time.deltaTime);
		_computeShader.SetFloat("_Time", Time.timeSinceLevelLoad);

		// Per Instance Data
		_computeShader.Dispatch(_kernelMoveParticles, _particleGroupsX, _particleGroupsY, 1);
		//_computeShader.Dispatch(_kernelResizeVerticesId, _particleGroupsX, _particleGroupsY, 1);

		// Per Vertex Data
		_computeShader.Dispatch(_kernelMeltParticlesId, (_numParticlesX*4*6)/kNumThreadsVerticesX, _numParticlesY/kNumThreadsVerticesY, 1);
	}

	GameObject CreateParticle(float id)
	{
		GameObject particleGO = new GameObject();
		Mesh mesh = new Mesh();

		Vector3 pmin = Vector3.one * -0.5f;
		Vector3 pmax = Vector3.one * 0.5f;

		mesh.vertices = new Vector3[4 * 6] {
			// Bottom
			  new Vector3(pmin.x, pmin.y, pmax.z), new Vector3(pmin.x, pmin.y, pmin.z), new Vector3(pmax.x, pmin.y, pmin.z), new Vector3(pmax.x, pmin.y, pmax.z)

			// Top
			, new Vector3(pmin.x, pmax.y, pmin.z), new Vector3(pmin.x, pmax.y, pmax.z), new Vector3(pmax.x, pmax.y, pmax.z), new Vector3(pmax.x, pmax.y, pmin.z)

			// Left
			, new Vector3(pmin.x, pmin.y, pmax.z), new Vector3(pmin.x, pmax.y, pmax.z), new Vector3(pmin.x, pmax.y, pmin.z), new Vector3(pmin.x, pmin.y, pmin.z)

			// Right
			, new Vector3(pmax.x, pmin.y, pmin.z), new Vector3(pmax.x, pmax.y, pmin.z), new Vector3(pmax.x, pmax.y, pmax.z), new Vector3(pmax.x, pmin.y, pmax.z)

			// Front
			, new Vector3(pmin.x, pmin.y, pmin.z), new Vector3(pmin.x, pmax.y, pmin.z), new Vector3(pmax.x, pmax.y, pmin.z), new Vector3(pmax.x, pmin.y, pmin.z)

			// Back
			, new Vector3(pmax.x, pmin.y, pmax.z), new Vector3(pmax.x, pmax.y, pmax.z), new Vector3(pmin.x, pmax.y, pmax.z), new Vector3(pmin.x, pmin.y, pmax.z)
		};

		mesh.vertices.CopyTo(_verticesData, _currentCopiedVertices);
		//_currentCopiedVertices += mesh.vertices.Length;


		mesh.normals = new Vector3[4 * 6] {
			// Bottom
			Vector3.down, Vector3.down, Vector3.down, Vector3.down
			
			// Top
			, Vector3.up, Vector3.up, Vector3.up, Vector3.up
			
			// Left
			, Vector3.left, Vector3.left, Vector3.left, Vector3.left
			
			// Right
			, Vector3.right, Vector3.right, Vector3.right, Vector3.right
			
			// Front
			, Vector3.back, Vector3.back, Vector3.back, Vector3.back
			
			// Back
			, Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward
		};

		mesh.colors = new Color[4 * 6] {
			// Bottom
			new Color(0.0f, 0.0f, 1.0f), new Color(0.0f, 0.0f, 0.0f), new Color(1.0f, 0.0f, 0.0f), new Color(1.0f, 0.0f, 1.0f)
			
			// Top
			, new Color(0.0f, 1.0f, 0.0f), new Color(0.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 0.0f)
			
			// Left
			, new Color(0.0f, 0.0f, 1.0f), new Color(0.0f, 1.0f, 1.0f), new Color(0.0f, 1.0f, 0.0f), new Color(0.0f, 0.0f, 0.0f)
			
			// Right
			, new Color(1.0f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f), new Color(1.0f, 0.0f, 1.0f)
			
			// Front
			, new Color(0.0f, 0.0f, 0.0f), new Color(0.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f), new Color(1.0f, 0.0f, 0.0f)
			
			// Back
			, new Color(1.0f, 0.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f), new Color(0.0f, 1.0f, 0.0f), new Color(0.0f, 0.0f, 1.0f)
		};

		mesh.uv = new Vector2[4 * 6] {
			// Bottom
			  new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)

			// Top
			, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)

			// Left
			, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)

			// Right
			, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)

			// Front
			, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)

			// Front
			, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1)
		};

		mesh.uv2 = new Vector2[4 * 6] {
			// Bottom
			new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++)
			
			// Top
			, new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++)
			
			// Left
			, new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++)
			
			// Right
			, new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++)
			
			// Front
			, new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++)
			
			// Front
			, new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++), new Vector2(((float)_currentCopiedVertices)/10000f, _currentCopiedVertices++)
		};

		mesh.triangles = new int[6 * 6] {
			// bottom
			  0, 1, 2
			, 0, 2, 3

			// top
			, 4, 5, 6
			, 4, 6, 7

			// left
			, 8, 9, 10
			, 8, 10, 11

			// right
			, 12, 13, 14
			, 12, 14, 15

			// Front
			, 16, 17, 18
			, 16, 18, 19

			// Front
			, 20, 21, 22
			, 20, 22, 23
		};

		MeshRenderer mr = particleGO.AddComponent<MeshRenderer>();
		mr.material = _particleMaterial;

		MeshFilter mf = particleGO.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		return particleGO;
	}

	void OnDestroy()
	{
		
		// Unity cry if the GPU buffer isn't manually cleaned
		_particlesBuffer.Release();
	}
}
