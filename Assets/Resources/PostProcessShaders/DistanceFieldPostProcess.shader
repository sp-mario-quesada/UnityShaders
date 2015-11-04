Shader "Hidden/DistanceFieldShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlurTex ("_BlurTex", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		colormask rgb

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
			uniform sampler2D _BlurTex;
			uniform sampler2D _CameraDepthTexture;
			//uniform sampler2D _CameraDepthNormalsTexture;

			fixed4 frag (v2f i) : SV_Target
			{
				float sceneDepth = Linear01Depth (tex2D(_CameraDepthTexture, i.sspos.xy).r);
				fixed4 scene = tex2D(_MainTex, i.uv);
				fixed4 blur = tex2D(_BlurTex, i.sspos.xy);
				
				float d = smoothstep(60, 100, sceneDepth*(_ProjectionParams.z-_ProjectionParams.y));
				// if it is too far get back
				d = lerp(d, 0, smoothstep(200, 250, sceneDepth*(_ProjectionParams.z-_ProjectionParams.y)));
				
				fixed4 finalColor = lerp(scene, blur, d);
				
				finalColor.a = scene.a;
				
				return finalColor;
			}
			ENDCG
		}
	}
}
