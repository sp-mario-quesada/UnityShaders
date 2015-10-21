Shader "Unlit/ParticleImageShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

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
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#pragma target 5.0
			
//			#define RIGHT half4(1, 0, 0, 0)
//			#define UP half4(0, 1, 0, 0)
			
			struct ParticleData
			{
				float3 position;
				float4 color;
			};

			struct appdata
			{
				float3 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
				fixed4 color : TEXCOORD1;
				float inst : TEXCOORD2;
			};
			
			struct g2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : TEXCOORD1;
			};
			
			StructuredBuffer<ParticleData> _ParticlesBuffer;
			float4 _ImageSize;
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ParticleHalfSize;
			float4 _WorldSpaceCameraRight;
			float4 _WorldSpaceCameraUp;
			
			v2g vert (appdata v, uint inst : SV_InstanceID, uint vid : SV_VertexID)
			{
				v2g o;
				
				float3 wpos = _ParticlesBuffer[inst].position;
	
				o.vertex = float4(wpos, 1);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.color = _ParticlesBuffer[inst].color;
				o.inst = inst;
				return o;
			}
			
			[maxvertexcount(4)]
			void geom(point v2g i[1], inout TriangleStream<g2f> oStream)
			{
				half4 hSize = _ParticleHalfSize;
				g2f o = (g2f) 0;
				
				//float rad = 3.1415*cos(i[0].inst);
				//float4 RIGHT = float4( cos(rad), 0, sin(rad), 0 );
				float4 RIGHT = float4( 1, 0, 0, 0 );
				float4 UP = float4(0, 1, 0, 0);
				
				o.vertex = mul(UNITY_MATRIX_VP, i[0].vertex -UP*hSize.y -RIGHT*hSize.x); o.uv = half2(0, 0); o.color = i[0].color; oStream.Append(o);
				o.vertex = mul(UNITY_MATRIX_VP, i[0].vertex +UP*hSize.y -RIGHT*hSize.x); o.uv = half2(0, 1); o.color = i[0].color; oStream.Append(o);
				o.vertex = mul(UNITY_MATRIX_VP, i[0].vertex -UP*hSize.y +RIGHT*hSize.x); o.uv = half2(1, 0); o.color = i[0].color; oStream.Append(o);
				o.vertex = mul(UNITY_MATRIX_VP, i[0].vertex +UP*hSize.y +RIGHT*hSize.x); o.uv = half2(1, 1); o.color = i[0].color; oStream.Append(o);
				
				oStream.RestartStrip();
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				fixed4 col = i.color; 
				
				return col;
			}
			ENDCG
		}
	}
}
