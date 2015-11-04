Shader "Hidden/SocialPoint/GlowFilterPostProcess"
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
			Stencil {
                Ref 100
                Comp notequal
            }
            
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

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed lum = Luminance(col.rgb);
				
				col = lerp(col, fixed4(0, 0, 0, col.a), step(col.a, 0.9999)); // only to non solid things
				col = lerp(fixed4(0, 0, 0, col.a), col, step(0.8, lum));
				
				half skyValue = 0.8;
				col = lerp(fixed4(0, 0, 0, col.a), col, step(0.001, abs(col.a-skyValue) ));
				
//				if(col.a > 0.99)
//				{
//					col = fixed4(0, 0, 0, 1);
//				}

				return col;
			}
			ENDCG
		}
	}
}
