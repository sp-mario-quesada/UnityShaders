Shader "Hidden/DepthDistorsionPostProcess"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("NoiseTex", 2D) = "white" {}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 sspos : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
				o.sspos = ComputeScreenPos(o.vertex);
				o.uv = v.uv;
				return o;
			}
			
			uniform sampler2D _MainTex;
			uniform sampler2D _NoiseTex;
			uniform sampler2D _CameraDepthTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				float sceneDepth = Linear01Depth (tex2D(_CameraDepthTexture, i.sspos.xy).r);
				fixed4 noise = tex2D(_NoiseTex, i.uv + half2(0, _Time.x*0.5))*2-1;
				
				float d = smoothstep(2.0, 10, sceneDepth*(_ProjectionParams.z-_ProjectionParams.y));
				d *= 0.002;
				
				half2 uvnoise = i.uv + noise.xy*d;
				fixed4 scene = tex2D(_MainTex, uvnoise);
				return scene;
			}
			ENDCG
		}
	}
}
