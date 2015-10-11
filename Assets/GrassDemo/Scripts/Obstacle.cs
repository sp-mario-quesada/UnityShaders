using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class Obstacle : MonoBehaviour 
{
	bool _isInit = false;

	Rigidbody _body;
	Collider _collider;

	void Init()
	{
		if(_isInit)
		{
			return;
		}
		_isInit = true;

		_body = GetComponent<Rigidbody>();
		_collider = GetComponentInChildren<Collider>();
	}

	public void Play(Vector3 force)
	{
		Init();

		_body.AddForce(force, ForceMode.VelocityChange);
	}

	public float GetRadius()
	{
		Init();

		return _collider.bounds.size.x;
	}
}
