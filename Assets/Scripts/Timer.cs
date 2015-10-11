using UnityEngine;
using System.Collections;

public struct Timer
{
	float _startTime;
	public float StartTime { get { return _startTime; } }

	float _endTime;
	public float EndTime { get { return _endTime; } }

	float _duration;
	public float Duration { get { return _duration; } }

	public float DeltaTime { get { return  Time.time - _startTime; } }

	public float NormalizedDeltaTime { get { return  Mathf.Clamp01(DeltaTime/_duration); } }

	public bool IsPlaying { get { return NormalizedDeltaTime < 1f; } }

	public void Wait(float duration)
	{
		_duration = duration;
		_startTime = Time.time;
		_endTime = Time.time + duration;
	}
}
