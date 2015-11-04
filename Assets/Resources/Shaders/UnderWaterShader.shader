Shader "SocialPoint/UnderWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaValue ("x:DistanceMin,y:DistanceMax", Vector) = (1, 1, 0, 0)
		_AlphaPivot ("Alpha Pivot", Vector) = (0.5,0.5,0,0)
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
		 "RenderType"="Opaque" 
		 "Queue" = "Geometry"
		}
		
		Pass
		{
			LOD 100	
			
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 wpos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _AlphaValue;
			half4 _AlphaPivot;
			half _PowFactor;
			half _MinDist;
			fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.wpos = mul(_Object2World, float4(v.vertex.xyz, 1)).xyz;
				o.pos = mul(UNITY_MATRIX_VP, float4(o.wpos, 1));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : Color
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				
				half distanceToPivot = length(i.wpos.xz - _AlphaPivot.xz);
				col.a = lerp(1, 0, smoothstep(_AlphaValue.x, _AlphaValue.y, distanceToPivot));
				
				return col;
			}
			ENDCG
		}
	}
	
	 FallBack "Diffuse"
	 //FallBack "SocialPoint/Basic"
}
