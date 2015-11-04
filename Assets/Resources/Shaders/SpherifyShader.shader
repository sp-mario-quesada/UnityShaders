Shader "SocialPoint/SpherifyShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SphereIntensity ("SphereIntensity", Float) = 1
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
		 "RenderType"="Opaque" 
		 "Queue" = "Geometry"
		 "LightMode" = "ForwardBase"
		}
		
		Pass
		{
			LOD 100	
		
			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Assets/Resources/ShaderIncludes/VertexUtils.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
				LIGHTING_COORDS(2,3)
//				float fogAmplitude : TEXCOORD4;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _SphereIntensity;
			half _PowFactor;
			half _MinDist;
			fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				float3 wpos = mul(_Object2World, float4(v.vertex.xyz, 1)).xyz;
				wpos = Spherify(float4(wpos, 1), _SphereIntensity).xyz;
				o.pos = mul(UNITY_MATRIX_VP, float4(wpos, 1));
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.pos);
				
//				float depthView = -mul(UNITY_MATRIX_MV, float4(v.vertex.xyz, 1)).z;
//				o.fogAmplitude = lerp(0, 1, smoothstep(4, 10, depthView));
				
				return o;
			}
			
			fixed4 frag (v2f i) : Color
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				
				// apply shadow
				float att = LIGHT_ATTENUATION(i);
				col *= max(att, 0.25);
				
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				
				//col = lerp(col, fixed4(0.5, 0.5, 0, 1), i.fogAmplitude);
				
				return col;
			}
			ENDCG
		}
	}
	
	 FallBack "Diffuse"
	 //FallBack "SocialPoint/Basic"
}
