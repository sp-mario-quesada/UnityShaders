using UnityEngine;
using System.Collections;

public class GlobalParameters : MonoBehaviour 
{

	void Update()
	{
		Shader.SetGlobalVector("_SunDir", -transform.forward.normalized);
	}
}
