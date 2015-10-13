using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class DistanceFieldPostProcess : MonoBehaviour 
{
	public Material _distanceFieldMat;

	public Material _blurMat;
	public Shader _blurShader;

	public Material _scaleMat;
	public Shader _scaleShader;

	RenderTexture _rt0;
	RenderTexture _rt1;

	void CreateAssets(int w, int h)
	{
		if(_distanceFieldMat == null)
		{
			_distanceFieldMat = Resources.Load<Material>("DistanceFieldMat");
		}

		if(_blurMat == null)
		{
			_blurShader = Resources.Load<Shader>("PostProcessShaders/Glow/GlowScalePostProcess");
			_blurMat = new Material(_blurShader);
		}

		if(_scaleMat == null)
		{
			_scaleShader = Resources.Load<Shader>("PostProcessShaders/Glow/ScalePostProcess");
			_scaleMat = new Material(_scaleShader);
		}

		if(_rt0 == null)
		{
			_rt0 = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
		}

		if(_rt1 == null)
		{
			_rt1 = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
		}
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		int scaleDownW = 1 << 1;
		int scaleDownH = 1 << 1;
		
		int w = src.width >> 1;
		int h = src.height >> 1;

		CreateAssets(w, h);

		// Scale Down
		RenderTexture temp0 = _rt0;
		RenderTexture temp1 = _rt1;

		_scaleMat.SetVector("_TextureResolution", new Vector4(w, h, 0, 0));
		Graphics.Blit(src, temp0, _scaleMat);

		// Blur
		for (int i = 0; i < 2; ++i) 
		{
			_blurMat.SetVector("_TextureResolution", new Vector4(w, h, 0, 0));
			_blurMat.SetVector("_BlurDirection", new Vector4(1, 0, 0, 0));
			Graphics.Blit(temp0, temp1, _blurMat);
			temp0 = temp1;

			_blurMat.SetVector("_TextureResolution", new Vector4(w, h, 0, 0));
			_blurMat.SetVector("_BlurDirection", new Vector4(0, 1, 0, 0));
			Graphics.Blit(temp0, temp1, _blurMat);
		}

		_distanceFieldMat.SetTexture("_BlurTex", temp1);
		Graphics.Blit(src, dest, _distanceFieldMat);
	}
}
