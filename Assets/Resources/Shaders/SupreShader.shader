Shader "SocialPoint/SupreShader"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_SpecularProps("x:Pow,y:Intensity", Vector) = (1,1,1,1)
	}
	
	SubShader
	{
			Tags
			{
				"Queue" = "Overlay"
				"RenderType" = "Transparent"
				
			}

		Pass
		{
						
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _MainTex;
			half4 _SpecularProps;
			
			struct vertexInput
			{
				float3 pos : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
			
			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD4;
				float3 normal : TEXCOORD3;
				float3 wpos : TEXCOORD5;
			};
			
			vertexOutput vert(vertexInput i)
			{
				vertexOutput output;
				float3 color = tex2Dlod(_MainTex, float4(i.uv, 0, 0)).rgb;
				float3 pos = i.pos + (0.5+0.5*sin(_Time.y * 10)) * (i.normal*2-1) * 0.0;
				output.pos = mul(UNITY_MATRIX_MVP, float4(pos.xyz, 1));
				output.uv = i.uv;
				output.normal = mul((float3x3)_Object2World, i.normal.xyz);
				output.wpos = mul(_Object2World, float4(pos.xyz, 1));
				
				return output;
			}
			
			fixed4 frag(vertexOutput i) : COLOR
			{
				float3 lightDir = _WorldSpaceLightPos0;
				float diffFactor = dot(i.normal, lightDir);
				
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.wpos);
				
				float3 lightDirReflected = -lightDir + 2*dot(lightDir, i.normal)*i.normal;
				float specularFactor = pow(max(dot(lightDirReflected, viewDir), 0), _SpecularProps.x) * _SpecularProps.y;
				
				float fresnel = 1-pow(max(dot(viewDir, i.normal),0), 10);
				
				fixed4 col = tex2D(_MainTex, i.uv) * diffFactor;// + fixed4(1,1,1,1) * specularFactor;
				
				return col;
			}
			
			ENDCG	
		}
	}
}