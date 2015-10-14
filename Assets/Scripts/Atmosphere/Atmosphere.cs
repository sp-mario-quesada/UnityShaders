using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Renderer))]
public class Atmosphere : MonoBehaviour 
{
	float _radius;
	public float Radius { get { return _radius; } }

	public void Init(float radius)
	{
		transform.localScale = Vector3.one * radius;
	}
}
