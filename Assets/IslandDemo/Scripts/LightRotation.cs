using UnityEngine;
using System.Collections;

public class LightRotation : MonoBehaviour 
{
	[SerializeField]
	float _speed = 10f;

	Quaternion _from;
	Quaternion _to;

	int _direction;

	void Awake()
	{
		_from = Quaternion.AngleAxis(0, Vector3.right);
		_to = Quaternion.AngleAxis(360, Vector3.right);
		_direction = 1;
	}

	void Update()
	{
		Quaternion rotation;

		rotation = Quaternion.AngleAxis(_speed * Time.deltaTime, Vector3.right);
		Quaternion finalRotation = rotation * transform.rotation;
		transform.rotation = finalRotation;

//		if(_direction == 1)
//		{
//			rotation = Quaternion.RotateTowards(transform.rotation, _to, _speed * Time.deltaTime);
//
//			if (Quaternion.Angle(transform.rotation, _to)<1f)
//			{
//				_direction = -_direction;
//			}
//		}
//		else
//		{
//			rotation = Quaternion.RotateTowards(transform.rotation, _from, _speed * Time.deltaTime);
//
//			if (Quaternion.Angle(transform.rotation, _from)<1f)
//			{
//				_direction = -_direction;
//			}
//		}
//		transform.rotation = rotation;
	}
}
