using UnityEngine;
using System.Collections;

namespace SocialPoint
{
	public enum WaterQualityType
	{
		SIMPLE = 0,
		REFRACTION,
		REFLECTION,
	};

//	[ExecuteInEditMode]
	public class SocialPointWater : MonoBehaviour 
	{
		public bool disablePixelLights = true;

		public WaterQualityType qualityType = WaterQualityType.REFLECTION;
		public int refractionTextureSize = 256;
		public int reflectionTextureSize = 256;
		public float m_ClipPlaneOffset = 0.07f;

		// refraction
		private RenderTexture m_RefractionTexture = null;
		private Camera refractionCamera;
		public LayerMask RefractionMask;

		// reflection
		private RenderTexture m_ReflectionTexture = null;
		private Camera reflectionCamera;
		public LayerMask ReflectionMask;

		public float degrees = 0.0f;

		Material material = null;

		void Start () 
		{
			material = GetMaterial();

			InitRefractionTexture();
			InitRefractionCamera();

			InitReflectionTexture();
			InitReflectionCamera();
		}

		void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if(cam == null)
			{
				return;
			}

			// Optionally disable pixel lights for reflection/refraction
			int oldPixelLightCount = QualitySettings.pixelLightCount;
			if( disablePixelLights )
			{
				QualitySettings.pixelLightCount = 0;
			}

			if(qualityType >= WaterQualityType.REFRACTION)
			{
				UpdateCameraModes(cam, refractionCamera);
				RenderRefraction(cam);
			}

			if(qualityType >= WaterQualityType.REFLECTION)
			{
				UpdateCameraModes(cam, reflectionCamera);
				RenderReflection(cam);
			}

			// Restore pixel light
			QualitySettings.pixelLightCount = oldPixelLightCount;
		
		}

		void InitRefractionTexture()
		{
			m_RefractionTexture = new RenderTexture( refractionTextureSize, refractionTextureSize, 16 );
			m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
			m_RefractionTexture.isPowerOfTwo = true;
			m_RefractionTexture.hideFlags = HideFlags.DontSave;
		}

		void InitRefractionCamera()
		{
			GameObject go = new GameObject( "Water Refraction Camera id" + GetInstanceID(), typeof(Camera), typeof(Skybox) ); // maybe we should delete the skybox

			refractionCamera = go.GetComponent<Camera>();
			refractionCamera.enabled = false;

			refractionCamera.gameObject.AddComponent<FlareLayer>();
			go.hideFlags = HideFlags.HideAndDontSave;
		}

		void InitReflectionTexture()
		{
			m_ReflectionTexture = new RenderTexture( reflectionTextureSize, reflectionTextureSize, 16 );
			m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
			m_ReflectionTexture.isPowerOfTwo = true;
			m_ReflectionTexture.hideFlags = HideFlags.DontSave;
		}

		void InitReflectionCamera()
		{
			GameObject go = new GameObject( "Water Reflection Camera id" + GetInstanceID(), typeof(Camera), typeof(Skybox) );

			reflectionCamera = go.GetComponent<Camera>();
			reflectionCamera.enabled = false;

			reflectionCamera.gameObject.AddComponent<FlareLayer>();
			go.hideFlags = HideFlags.HideAndDontSave;
		}

		private void UpdateCameraModes( Camera src, Camera dest )
		{
			if( dest == null )
				return;
			// set water camera to clear the same way as current camera
			dest.clearFlags = src.clearFlags;
			dest.backgroundColor = src.backgroundColor;		
			if( src.clearFlags == CameraClearFlags.Skybox )
			{
				Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
				Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
				if( !sky || !sky.material )
				{
					mysky.enabled = false;
				}
				else
				{
					mysky.enabled = true;
					mysky.material = sky.material;
				}
			}
			// update other values to match current camera.
			// even if we are supplying custom camera&projection matrices,
			// some of values are used elsewhere (e.g. skybox uses far plane)
			dest.farClipPlane = src.farClipPlane;
			dest.nearClipPlane = src.nearClipPlane;
			dest.orthographic = src.orthographic;
			dest.fieldOfView = src.fieldOfView;
			dest.aspect = src.aspect;
			dest.orthographicSize = src.orthographicSize;
		}

		Material GetMaterial()
		{
			MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

			return meshRenderer.sharedMaterial;
		}

		public Vector3 cameraOfsset;

		// Given position/normal of the plane, calculates plane in camera space.
		private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
			Matrix4x4 m = cam.worldToCameraMatrix;
			Vector3 cpos = m.MultiplyPoint( offsetPos );
			cpos += cameraOfsset;
			Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
			return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
		}

		// Adjusts the given projection matrix so that near plane is the given clipPlane
		// clipPlane is given in camera space. See article in Game Programming Gems 5 and
		// http://aras-p.info/texts/obliqueortho.html
		private static void CalculateObliqueMatrix (ref Matrix4x4 projection, Vector4 clipPlane)
		{
			Vector4 q = projection.inverse * new Vector4(
				sgn(clipPlane.x),
				sgn(clipPlane.y),
				1.0f,
				1.0f
				);
			Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
			// third row = clip plane - fourth row
			projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];
		}

		// Extended sign: returns -1, 0 or 1 based on sign of a
		private static float sgn(float a)
		{
			if (a > 0.0f) return 1.0f;
			if (a < 0.0f) return -1.0f;
            return 0.0f;
        }
        
        void RenderRefraction(Camera cam)
		{
			refractionCamera.transform.position = cam.transform.position;
			refractionCamera.transform.rotation = cam.transform.rotation;
			refractionCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

			// Setup oblique projection matrix so that near plane is our reflection
			// plane. This way we clip everything below/above it for free.
			//Vector4 clipPlane = CameraSpacePlane( refractionCamera, transform.position, transform.up, -1.0f ); // needed ?
			Matrix4x4 projection = cam.projectionMatrix;
			//CalculateObliqueMatrix (ref projection, clipPlane); // needed ?
			refractionCamera.projectionMatrix = projection;

			refractionCamera.cullingMask = ~(1 << gameObject.layer) & (RefractionMask.value); // never render water layer

			refractionCamera.targetTexture = m_RefractionTexture;
			refractionCamera.transform.position = cam.transform.position;
			refractionCamera.transform.rotation = cam.transform.rotation;
			refractionCamera.Render();

			material.SetTexture("_RefractionTex", m_RefractionTexture);
		}

		void RenderReflection(Camera cam)
		{
			// find out the reflection plane: position and normal in world space
			Vector3 pos = transform.position;
			Vector3 normal = transform.up;

			UpdateCameraModes( cam, refractionCamera );

			// Reflect camera around reflection plane
			float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
			Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
			
			Matrix4x4 reflection = Matrix4x4.zero;
			CalculateReflectionMatrix (ref reflection, reflectionPlane);
			Vector3 oldpos = cam.transform.position;
			Vector3 newpos = reflection.MultiplyPoint( oldpos );
			reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
			
			// Setup oblique projection matrix so that near plane is our reflection
			// plane. This way we clip everything below/above it for free.
			Vector4 clipPlane = CameraSpacePlane( reflectionCamera, pos, normal, 1.0f );
			Matrix4x4 projection = cam.projectionMatrix;
			CalculateObliqueMatrix (ref projection, clipPlane);
			reflectionCamera.projectionMatrix = projection;
			
			//reflectionCamera.cullingMask = ~(1<<4) & m_ReflectLayers.value; // never render water layer
			reflectionCamera.cullingMask = ~(1 << gameObject.layer) & (ReflectionMask.value); // never render water layer
			reflectionCamera.targetTexture = m_ReflectionTexture;
			GL.SetRevertBackfacing (true);
			reflectionCamera.transform.position = newpos;
			Vector3 euler = cam.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

			Quaternion zrot = Quaternion.AngleAxis (degrees, new Vector3(0,0,1));
			Quaternion newRot = reflectionCamera.transform.rotation;
			newRot = newRot * zrot;
			reflectionCamera.transform.rotation = newRot;

			reflectionCamera.Render();

			reflectionCamera.transform.position = oldpos;
			GL.SetRevertBackfacing (false);
			material.SetTexture( "_ReflectionTex", m_ReflectionTexture );
		}

		// Calculates reflection matrix around the given plane
		private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
			reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
			reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
			reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);
			
			reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
			reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
			reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
			reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);
			
			reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
			reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
			reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
			reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);
			
			reflectionMat.m30 = 0F;
			reflectionMat.m31 = 0F;
			reflectionMat.m32 = 0F;
			reflectionMat.m33 = 1F;
		}
	}
}
