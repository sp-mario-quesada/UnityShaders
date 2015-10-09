using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CellsPostProcess : MonoBehaviour 
{
	[SerializeField]
	Shader _shader;
	Material _mat;


	void CreateResources(int w, int h)
	{
		if(_mat == null)
		{
			_mat = new Material(_shader);
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

		Graphics.Blit(src, dest, _mat);
	}
}
