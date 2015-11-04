Shader "Hidden/SocialPoint/GlowScalePostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		colormask rgb
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
			float4 _TextureResolution;
			half4 _BlurDirection;
			
			fixed4 frag (v2f i) : SV_Target
			{
				float kernel[BLUR1D_KERNEL_SIZE];
				float2 offfset[BLUR1D_KERNEL_SIZE];
				GetBlurData1D(_TextureResolution.x, _TextureResolution.y, kernel, offfset, _BlurDirection.xy);
				
				fixed4 sum = float4(0,0,0,0);
				for(int idx = 0; idx < BLUR1D_KERNEL_SIZE; ++idx)
				{
					sum += tex2D(_MainTex, i.uv + offfset[idx] + GetHalfPixel(_TextureResolution.x, _TextureResolution.y)) * kernel[idx];
				}
				return sum;
			}
			ENDCG
		}
	}
}
