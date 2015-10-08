Shader "SocialPoint/GPUParticleShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LightPos ("Light Position", Vector) = (0, 10, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct Particle
			{
				float3 position;
				float3 velocity;
				float3 size;
			};

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 id : TEXCOORD2;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 vpos : TEXCOORD1;
			};

			uniform StructuredBuffer<Particle> _ParticlesBuffer;
			uniform StructuredBuffer<float3> _Vertices;
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _LightPos;
			
			v2f vert (appdata v, uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
				
				int vertexId = round(v.id.x*10000.0);
				int instanceId = floor(vertexId/(4*6.0));
				
				float3 vertex = v.vertex * _ParticlesBuffer[instanceId].size;
				float3 instancePos = _ParticlesBuffer[instanceId].position;
				float3 wpos = instancePos + vertex;
				
				o.vertex = mul(UNITY_MATRIX_VP, float4(wpos, 1.0));
				o.vpos = mul(UNITY_MATRIX_V, wpos);
				
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= i.color;
				
				col.rgb = lerp(col, fixed3(sin(i.vpos.x), sin(i.vpos.y), sin(i.vpos.z)), 0.5);
				
				return col;
			}
			ENDCG
		}
	}
}
