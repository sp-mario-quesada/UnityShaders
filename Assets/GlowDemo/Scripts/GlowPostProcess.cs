using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
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

	RenderTexture _rt0;
	RenderTexture _rt1;

	void CreateResources(int w, int h)
	{
		if(_rt0 == null)
		{
			_rt0 = CreateRenderTexture(Mathf.RoundToInt(w/((float)_scaleDownStep0)), Mathf.RoundToInt(h/((float)_scaleDownStep0)));
		}
		if(_rt1 == null)
		{
			_rt1 = CreateRenderTexture(Mathf.RoundToInt(w/((float)_scaleDownStep1)), Mathf.RoundToInt(h/((float)_scaleDownStep1)));
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

		_filterMat.SetVector("_TextureResolution", new Vector4(src.width, src.height, 0, 0));
		Graphics.Blit(src, _rt0, _filterMat);

		_scaleMat.SetVector("_TextureResolution", new Vector4(_rt0.width, _rt0.height, 0, 0));
		Graphics.Blit(_rt0, _rt1, _scaleMat);

		_mergeMat.SetTexture("_SceneTex", src);
		_mergeMat.SetVector("_TextureResolution", new Vector4(_rt1.width, _rt1.height, 0, 0));
		Graphics.Blit(_rt1, dest, _mergeMat);
	}
}
