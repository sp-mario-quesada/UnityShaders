using UnityEngine;
using System.Collections;

public abstract class BaseAction : MonoBehaviour
{
	public void Play()
	{
		DoPlay();
	}
	public abstract void DoPlay();

	public bool IsPlaying()
	{
		return DoIsPlaying();
	}
	public abstract bool DoIsPlaying();
}
