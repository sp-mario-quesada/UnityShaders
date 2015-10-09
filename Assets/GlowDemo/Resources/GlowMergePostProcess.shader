Shader "Hidden/SocialPoint/GlowMergePostProcess"
{
	Properties
	{
		_MainTex ("Glow Texture", 2D) = "white" {}
		_GlowScaledDownTex ("GlowScaledDown", 2D) = "white" {}
		_GlowFactor ("GlowFactor", Float) = 1
		_InvertGlowTexYCoord ("Invert Glow Tex YCoord", Float) = 1
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
			#include "Assets/Resources/ShaderIncludes/PostProcessHelper.cginc"

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
			sampler2D _GlowScaledDownTex;
			
			float4 _TextureResolution;
			fixed _GlowFactor;
			fixed _InvertGlowTexYCoord;

			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 scene = tex2D(_MainTex, i.uv + GetHalfPixel(_ScreenParams.x, _ScreenParams.y));
				
				float kernel[BLUR_KERNEL_SIZE];
				float2 offfset[BLUR_KERNEL_SIZE];
				GetBlurData(_TextureResolution.x, _TextureResolution.y, kernel, offfset);

				float2 glowuv = lerp(i.uv, float2(i.uv.x, 1-i.uv.y), step(0.5, _InvertGlowTexYCoord));
				fixed4 sum = float4(0,0,0,0);
				for(int idx = 0; idx < BLUR_KERNEL_SIZE; ++idx)
				{
					sum += tex2D(_GlowScaledDownTex, glowuv + offfset[idx] + GetHalfPixel(_TextureResolution.x, _TextureResolution.y)) * kernel[idx];
				}
				fixed4 col = (sum + scene);
				return col;
			}
			ENDCG
		}
	}
}
