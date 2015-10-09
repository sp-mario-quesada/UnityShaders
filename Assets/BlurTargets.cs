using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlurTargets : MonoBehaviour 
{
	[SerializeField]
	GlowPostProcess _glowPostProcess;

	[SerializeField]
	RawImage _filter;
	[SerializeField]
	RawImage _rt0;
	[SerializeField]
	RawImage _rt1;
	[SerializeField]
	RawImage _rt2;
	[SerializeField]
	RawImage _rt3;
	[SerializeField]
	RawImage _merge;

	public void Start()
	{
		_filter.texture = _glowPostProcess._rtFilter;

		_rt0.texture = _glowPostProcess._rtDown0H;
		_rt1.texture = _glowPostProcess._rtDOwn0HV;
		_rt2.texture = _glowPostProcess._rtDown1H;
		_rt3.texture = _glowPostProcess._rtDown1HV;

		_merge.texture = _glowPostProcess._rtMerge0;
	}
}
