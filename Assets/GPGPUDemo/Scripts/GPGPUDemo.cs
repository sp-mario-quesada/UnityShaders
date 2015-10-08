using UnityEngine;
using System.Collections;

public class GPGPUDemo : MonoBehaviour 
{
	GPUParticleSystem _particleSystem;

	void Awake()
	{
		_particleSystem	= GetComponent<GPUParticleSystem>();
		_particleSystem.Init();
	}
}
