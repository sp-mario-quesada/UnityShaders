Shader "SocialPoint/GrassTerrainShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalMap ("Normal", 2D) = "white" {}
		_SphereIntensity ("SphereIntensity", Float) = 1
		_Color ("Color", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (0,0,0,1)
		_FresnelColor ("Fresnel Color", Color) = (0,0,0,1)
		_Overbright ("_Overbright", Float) = 1
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
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "Assets/Resources/ShaderIncludes/VertexUtils.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 pos : SV_POSITION;
				LIGHTING_COORDS(2,3)
				float3 wpos : TEXCOORD4;
				float3 normal : TEXCOORD5;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NormalMap;
			half _SphereIntensity;
			half _PowFactor;
			half _MinDist;
			fixed4 _Color;
			fixed4 _Color2;
			fixed4 _FresnelColor;
			half _Overbright;
			
			v2f vert (appdata v)
			{
				v2f o;
				float3 wpos = mul(_Object2World, float4(v.vertex.xyz, 1)).xyz;
				o.wpos = Spherify(float4(wpos, 1), _SphereIntensity).xyz;
				o.pos = mul(UNITY_MATRIX_VP, float4(wpos, 1));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = mul((float3x3)_Object2World, v.normal.xyz);
				
				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				return o;
			}
			
			fixed4 frag (v2f i) : Color
			{
				// sample the texture
				fixed4 texColor = tex2D(_MainTex, i.uv*half2(2,2));
				fixed4 normalMap = tex2D(_NormalMap, i.uv*half2(3,3))*2-1;
				
				half d = 0.5+0.5*sin(i.wpos.z*0.7) * 0.5+0.5*sin(i.wpos.x*0.8);
				d *= 0.5+0.5*sin(length(i.wpos.xz)*0.7);
				fixed4 color = lerp(_Color, _Color2, smoothstep(0, 1, d));
				color *= texColor * _Overbright;
				
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz-i.wpos);
				half fresnel = pow(1-max(dot(viewDir, i.normal+normalMap*0.2), 0), 2) * 0.5;
				color += _FresnelColor * fresnel;
				
				half3 halfVector = normalize((viewDir + _WorldSpaceLightPos0) * 0.5);
				half spec = pow(dot(halfVector, i.normal+normalMap*0.2), 4);
				color += spec * _LightColor0 * 0.5;
				
				// apply shadow
				float att = LIGHT_ATTENUATION(i);
				color *= max(att, 0.25);
				
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, color);
				return color;
			}
			ENDCG
		}
	}
	
	 FallBack "Diffuse"
	 //FallBack "SocialPoint/Basic"
}
