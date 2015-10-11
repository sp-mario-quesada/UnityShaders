using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class Obstacle : MonoBehaviour 
{
	[SerializeField]
	BaseAction _explosionForceAnimation;

	[SerializeField]
	float _minCollisionVel2ExpandFoce = 8f;

	Rigidbody _body;

	Collider _collider;

	Vector3 _prevVel;
	bool _hasImpacted = false;

	float _exansiveForce = 0f;
	public float ExpansiveForce { get { return _exansiveForce; } }

	public void Init()
	{
		_hasImpacted = false;
		_collider = GetComponentInChildren<Collider>();
		_body = GetComponent<Rigidbody>();
	}

	public void Play(Vector3 force)
	{
		_body.AddForce(force, ForceMode.VelocityChange);
	}

	public float GetRadius()
	{
		return _collider.bounds.size.x;
	}

	void OnCollisionEnter(Collision other)
	{
		if(other.relativeVelocity.sqrMagnitude < _minCollisionVel2ExpandFoce)
		{
			return;
		}

		_explosionForceAnimation.Play();
	}

	public void OnExplosionForceUpdate(float forceValue)
	{
		_exansiveForce = forceValue;
	}
}
