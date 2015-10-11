Shader "SocialPoint/DiffuseShader"
{

	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		
		_SpecularColor("Spec Color", Color) = (1,1,1,1)
		_SpecProps("x:SpecPow, y:SpecIntensity)", Vector) = (10,0,0,0)
		
		_EffectProps("x:GlowFactor)", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Lighting.cginc"
			
			sampler2D _MainTex;
			fixed4 _Color;
			
			fixed4 _SpecularColor;
			half4 _SpecProps;
			
			struct vertexdata
			{
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD1;
				float3 wpos : TEXCOORD2;
			};
			
			v2f vert(vertexdata i)
			{
				v2f o = (v2f) 0;
				o.pos = mul(UNITY_MATRIX_MVP, i.pos);
				o.wpos = mul(_Object2World, i.pos);
				o.normal = mul((float3x3)_Object2World, i.normal);
				o.texcoord = i.texcoord;
				
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 albedo = tex2D(_MainTex, i.texcoord) * _Color;
			
				half3 lightDir = _WorldSpaceLightPos0.xyz;
				half3 viewDir = normalize(_WorldSpaceCameraPos - i.wpos);
				
				// diffusecolor
				fixed diffFactor = max(dot(i.normal, lightDir), 0);
				
				// specular
				half3 halfv = normalize((viewDir+lightDir)*0.5);
				fixed specFactor = pow(max(dot(halfv, i.normal), 0), _SpecProps.x) * _SpecProps.y;
				
				fixed4 finalColor;
				finalColor.rgb = (albedo.rgb*diffFactor + _SpecularColor*specFactor*sign(diffFactor)) * _LightColor0.rgb;
				finalColor.a = albedo.a;
				
				return finalColor;
			}
			
			ENDCG
		}
	
		Pass
		{
			ColorMask A
			
			CGPROGRAM
			
			#include "UnityCG.cginc"
			
			#pragma vertex vert
			#pragma fragment frag
			
			half4 _EffectProps;
			
			struct v2f
			{
				float4 pos : POSITION;
			};
			
			v2f vert(appdata_base i)
			{
				v2f o = (v2f) 0;
				o.pos = mul(UNITY_MATRIX_MVP, float4(i.vertex.xyz, 1));
				return o;
			}
			
			fixed4 frag(appdata_base i) : COLOR
			{
				return fixed4(0,0,0, 1-_EffectProps.x);
			}
			
			ENDCG
		}
	}
}