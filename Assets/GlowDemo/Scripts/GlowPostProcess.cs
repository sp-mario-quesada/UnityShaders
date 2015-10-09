using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GlowPostProcess : MonoBehaviour 
{
	[SerializeField]
	int _scaleDownStep0 = 8;

	[SerializeField]
	int _scaleDownStep1 = 16;

	[SerializeField]
	Shader _filterShader;
	Material _filterMat;

	[SerializeField]
	Shader _scaleShader;
	Material _scaleMat;

	[SerializeField]
	Shader _mergeShader;
	Material _mergeMat;

	public RenderTexture _rtFilter;

	public RenderTexture _rtDown0H;
	public RenderTexture _rtDOwn0HV;

	public RenderTexture _rtDown1H;
	public RenderTexture _rtDown1HV;

	public RenderTexture _rtMerge0;

	void CreateResources(int w, int h)
	{
		if(_rtFilter == null)
		{
			_rtFilter = CreateRenderTexture(w, h);
		}
		if(_rtDown0H == null)
		{
			_rtDown0H = CreateRenderTexture(w/_scaleDownStep0, h);
		}
		if(_rtDOwn0HV == null)
		{
			_rtDOwn0HV = CreateRenderTexture(_rtDown0H.width, h/_scaleDownStep0);
		}
		if(_rtDown1H == null)
		{
			_rtDown1H = CreateRenderTexture( w/_scaleDownStep1, _rtDOwn0HV.height);
		}
		if(_rtDown1HV == null)
		{
			_rtDown1HV = CreateRenderTexture(_rtDown1H.width, h/_scaleDownStep1);
		}
		if(_rtMerge0 == null)
		{
			_rtMerge0 = CreateRenderTexture(_rtDOwn0HV.width, _rtDOwn0HV.height);
		}

		if(_scaleMat == null)
		{
			_scaleMat = new Material(_scaleShader);
		}
		if(_mergeMat == null)
		{
			_mergeMat = new Material(_mergeShader);
		}
		if(_filterMat == null)
		{
			_filterMat = new Material(_filterShader);
		}
	}

	RenderTexture CreateRenderTexture(int w, int h)
	{
		RenderTexture rt = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
		rt.enableRandomWrite = true;
		rt.Create();


		return rt;
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		CreateResources(src.width, src.height);

		// Scale Down
		_filterMat.SetVector("_TextureResolution", new Vector4(_rtFilter.width, _rtFilter.height, 0, 0));
		Graphics.Blit(src, _rtFilter, _filterMat);
	
		_scaleMat.SetVector("_TextureResolution", new Vector4(_rtFilter.width, _rtDown0H.height, 0, 0));
		_scaleMat.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
		Graphics.Blit(_rtFilter, _rtDown0H, _scaleMat);

		_scaleMat.SetVector("_TextureResolution", new Vector4(_rtDown0H.width, _rtDOwn0HV.height, 0, 0));
		_scaleMat.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
		Graphics.Blit(_rtDown0H, _rtDOwn0HV, _scaleMat);

		_scaleMat.SetVector("_TextureResolution", new Vector4(_rtDOwn0HV.width, _rtDown1H.height, 0, 0));
		_scaleMat.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
		Graphics.Blit(_rtDOwn0HV, _rtDown1H, _scaleMat);

		_scaleMat.SetVector("_TextureResolution", new Vector4(_rtDown1H.width, _rtDown1HV.height, 0, 0));
		_scaleMat.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
		Graphics.Blit(_rtDown1H, _rtDown1HV, _scaleMat);

		// Merge
		_mergeMat.SetTexture("_GlowScaledDownTex", _rtDown1HV);
		_mergeMat.SetFloat("_InvertGlowTexYCoord", 0f);
		_mergeMat.SetVector("_TextureResolution", new Vector4(_rtDown1HV.width, _rtDown1HV.height, 0, 0));
		Graphics.Blit(_rtDOwn0HV, _rtMerge0, _mergeMat);

		_mergeMat.SetTexture("_GlowScaledDownTex", _rtMerge0);
		_mergeMat.SetVector("_TextureResolution", new Vector4(_rtMerge0.width, _rtMerge0.height, 0, 0));
		_mergeMat.SetFloat("_InvertGlowTexYCoord", 1f);
		Graphics.Blit(src, dest, _mergeMat);
	}
}
