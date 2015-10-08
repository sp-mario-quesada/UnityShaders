Shader "Hidden/SocialPoint/GlowMergePostProcess"
{
	Properties
	{
		_MainTex ("Glow Texture", 2D) = "white" {}
		_SceneTex ("Scene Tex", 2D) = "white" {}
		_GlowFactor ("GlowFactor", Float) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			#define NUM_TAPS 4

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _SceneTex;
			
			float4 _TextureResolution;
			fixed _GlowFactor;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 taps[NUM_TAPS] = { float2(-1.5, 0), float2(0, 1.5), float2(1.5, 0), float2(0, -1.5)};
				
				fixed4 scene = tex2D(_SceneTex, i.uv);
				fixed4 glowcolor = tex2D(_MainTex, i.uv);
				
				fixed4 col = scene + glowcolor;
				return col;
			}
			ENDCG
		}
	}
}
