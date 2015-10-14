using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ExplosionDisplacementMap : MonoBehaviour 
{
	Material _material;

	void CreateAssets()
	{
		if(_material == null)
		{
			_material = Resources.Load<Material>("ExplosionDisplacementMapMat");
		}
	}

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		CreateAssets();

		Graphics.Blit(src, dest, _material);
	}
}
