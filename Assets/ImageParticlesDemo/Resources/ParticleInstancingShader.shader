Shader "Unlit/ParticleInstancing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#pragma target 5.0
			
			struct ParticleData
			{
				float3 position;
				float4 color;
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : TEXCOORD1;
			};
			
			
			StructuredBuffer<ParticleData> _ParticlesBuffer;
			float4 _ImageSize;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v, uint inst : SV_InstanceID, uint vid : SV_VertexID)
			{
				v2f o;
				
				float3 wpos = _ParticlesBuffer[inst].position;
	
				o.vertex = mul(UNITY_MATRIX_VP, float4(wpos, 1));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.color = _ParticlesBuffer[inst].color;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color; 
				
				return col;
			}
			ENDCG
		}
	}
}
