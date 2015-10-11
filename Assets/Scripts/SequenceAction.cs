using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequenceAction : BaseAction
{
	enum State
	{
		Idle,
		Playing
	}

	State _state = State.Idle;

	[SerializeField]
	List<BaseAction> _actions = new List<BaseAction>();
	List<BaseAction> Actions
	{
		get
		{
			if(_actions.Count == 0)
			{
				_actions = new List<BaseAction>(transform.GetComponents<BaseAction>());
				_actions.Remove(this);
			}

			return _actions;
		}
	}

	int _currentIdx = 0;

	public void AddAction(BaseAction action)
	{
		_actions.Add(action);
	}

	public override void DoPlay()
	{
		_currentIdx = 0;
		Actions[_currentIdx].Play();

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
		if(!Actions[_currentIdx].IsPlaying())
		{
			if(_currentIdx+1 < Actions.Count)
			{
				_currentIdx++;
				Actions[_currentIdx].Play();
			}
			else
			{
				_currentIdx = 0;
				_state = State.Idle;
			}
		}
	}
}
