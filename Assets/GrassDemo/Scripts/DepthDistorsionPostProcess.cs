using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class DepthDistorsionPostProcess : MonoBehaviour 
{
	public Material _distorsionMat;

	void CreateAssets()
	{
		if(_distorsionMat == null)
		{
			_distorsionMat = Resources.Load<Material>("DepthDistorsionMat");
		}
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		CreateAssets();

		Graphics.Blit(src, dest, _distorsionMat);
	}
}
