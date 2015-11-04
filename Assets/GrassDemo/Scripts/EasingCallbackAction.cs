using UnityEngine;
using System.Collections;

public class EasingCallbackAction : BaseAction
{
	enum State
	{
		Idle,
		Playing
	}

	[SerializeField]
	MonoBehaviour _scriptableObject;

	[SerializeField]
	string _methodName;

	[SerializeField]
	EasingType _easingType = EasingType.easeOutExpo;

	[SerializeField]
	float _duration;

	[SerializeField]
	float _from;

	[SerializeField]
	float _to;

	IEasing _easing;

	Timer _timer = new Timer();

	State _state = State.Idle;

	float _value = 0f;
	public float Value { get { return _value; } }

	public void SetProperties(EasingType easingType, float duration)
	{
		_easingType = easingType;
		_duration = duration;
	}

	public override void DoPlay()
	{
		_easing = EasingCore.Instance.GetEasing(_easingType);

		_value = _from;

		_timer.Wait(_duration);

		_state = State.Playing;
	}

	public override bool DoIsPlaying ()
	{
		return _state != State.Idle;
	}

	void Update()
	{
		switch (_state) 
		{
		case State.Idle:
			IdleState();
			break;

		case State.Playing:
			PlayingState();
			break;
		}
	}

	void IdleState()
	{}

	void PlayingState()
	{
		if(_timer.IsPlaying)
		{
			_value = Mathf.Lerp(_from, _to, _easing.ease(_timer.DeltaTime, 0f, 1f, _timer.Duration));
		}
		else
		{
			_value = _to;

			_state = State.Idle;
		}

		CallScriptable(Value);
	}

	void CallScriptable(float value)
	{
		_scriptableObject.SendMessage(_methodName, value);
	}
}
