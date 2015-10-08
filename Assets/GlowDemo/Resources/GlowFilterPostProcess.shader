Shader "Hidden/SocialPoint/GlowFilterPostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			float4 _TextureResolution;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 taps[NUM_TAPS] = { float2(-1.5, 0), float2(0, 1.5), float2(1.5, 0), float2(0, -1.5)};
				
				fixed4 center = tex2D(_MainTex, i.uv);
				fixed4 sum = center;
				for(int idx = 0; idx < NUM_TAPS; ++idx)
				{
					float2 uv = i.uv + taps[idx] / _TextureResolution.xy;
					sum += tex2D(_MainTex, uv);
				}
				
				fixed4 col = sum/(NUM_TAPS+1.0);
				
				if(center.a > 0.99)
				{
					col = fixed4(0, 0, 0, col.a);
				}
				
				return col;
			}
			ENDCG
		}
	}
}
