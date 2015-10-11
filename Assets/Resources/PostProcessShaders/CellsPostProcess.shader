Shader "Hidden/SocialPoint/CellsPostProcess"
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
				float numCells = 10 + 100 * (0.5+0.5*sin(_Time.y));
				float invNumCells = 1.0 / numCells;
			
				float2 cellId = i.uv/float2(invNumCells, invNumCells);
				float2 uv = floor(cellId)/numCells;
			
				fixed4 col = tex2D(_MainTex, uv);
				
				float d = min(abs(frac(cellId.x))-0.02, abs(frac(cellId.y))-0.02);
				fixed4 paintedCells = lerp(fixed4(0, 0, 0, 1), col, smoothstep(-0.01, 0.01, d));
				col = lerp(paintedCells, col, smoothstep(10, 30, numCells));
				
				return col;
			}
			ENDCG
		}
	}
}
