Shader "SocialPoint/SocialPointWater"
{
	//  Vertex Movement Height are scaled by VertexColor.r
	//  Alpha by VertexColor.g
	Properties
	{
		_ControlTex ("Reflective Color", 2D) = "white" {}
		
		_NormalMapAnimation("x:_NormalMapAnimation.U, y:_NormalMapAnimation.V", Vector) = (0.5, 0.5, 0, 0)
		
		_Cube ("Cube", Cube) = "white" {}
		_NormalMap ("NormalMap", 2D) = "black" {}
		_ReflectionProperties ("x:ReflPower(1-100),y:ReflDist(0-1),z:RefractDist(0-1),w:Nothing", Vector) = (1,0.01,0.01,0)
		_LightProperties ("x:specPower(1-100),y:SpecInt(0-4)", Vector) = (2,1.0,1.0,1)
		
		_UVScale ("UVScale", Vector) = (1,1,1,1)
		_WaveScale ("x:WavePhaseScale.x,y:WavePhaseScale.y,z:WaveHightScale,w:WaveSpeed", Vector) = (30,30,1,1)
		
		_HorizonColor ("_HorizonColor", Color) = (0,0,0,0)
		
		_ReflectionTex ("_ReflectionTex", 2D) = "white" {}
		_RefractionTex ("_RefractionTex", 2D) = "white" {}
	}
	SubShader
	{
		Tags {
		 "Queue"="Transparent+100" 
		 "RenderType"="Transparent" 
		 "LightMode"="ForwardBase" 
		 }

		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
		
			CGPROGRAM
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float3 color: COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed3 color: COLOR;
				float3 norm : NORMAL;
				float4 uvNormalMap : TEXCOORD0;
				float3 wpos : TEXCOORD1;
				float4 sspos : TEXCOORD2;
				LIGHTING_COORDS(3,4)
			};

			sampler2D _ControlTex;
			
			sampler2D _NormalMap;
			float4 _NormalMap_ST;
			half4 _NormalMapAnimation;
			
			samplerCUBE _Cube;
			half4 _ReflectionProperties;
			half4 _LightProperties;
			
			half4 _UVScale;
			half4 _WaveScale;
			
			fixed4 _HorizonColor;
			
			sampler2D _ReflectionTex;
			sampler2D _RefractionTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.color = v.color;
				
				float4 vertex = v.vertex;
				
				o.wpos = mul(_Object2World, float4(vertex.xyz, 1));
				
				half4 uv = o.wpos.xzxz;
				uv.xy *= _UVScale.xy;
				uv.zw *= _UVScale.zw;
				half2 uvScaleHalf = (_UVScale.xy + _UVScale.zw)*0.5;
				
				o.wpos.y += sin(o.wpos.x*uvScaleHalf.x*_WaveScale.x + _NormalMapAnimation.xy * _WaveScale.w * _Time.y) * sin(o.wpos.z*uvScaleHalf.y*_WaveScale.y + _NormalMapAnimation.y * _WaveScale.w * _Time.y) *_WaveScale.z * v.color.r ;
				
				o.pos = mul(UNITY_MATRIX_VP, float4(o.wpos.xyz, 1));
				o.norm = normalize( mul((float3x3) _Object2World, v.normal.xyz) );
				o.sspos = o.pos;
				
				o.uvNormalMap.xy = uv.xy + _NormalMapAnimation.xy * _Time.x;
				o.uvNormalMap.zw = uv.zw + _NormalMapAnimation.zw * _Time.x;
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 sspos = ((i.sspos.xy/i.sspos.w)+1)*0.5;
				sspos.y = 1 - sspos.y;
			
				// Vectors
				half3 norm = normalize(i.norm);
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wpos.xyz);
				half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
//				half3 reflVector = normalize(-viewDir + 2.0*dot(viewDir, norm)*norm);
				
				// Fresnel
				half fresnel = max(dot(viewDir, norm), 0);
				half rim = 1-fresnel;
				
				// Texture Fetch
				half3 normalMap1 = UnpackNormal(tex2D(_NormalMap, i.uvNormalMap.xy)).xyz;
				half3 normalMap2 = UnpackNormal(tex2D(_NormalMap, i.uvNormalMap.zw)).xyz;
				half3 normalMap = (normalMap1 + normalMap2) * 0.5;
				
				fixed4 waterColor = tex2D(_ControlTex, half2(rim, rim));
					   waterColor = lerp(waterColor, _HorizonColor, waterColor.a);
				
				half2 uv1 = sspos.xy; uv1.xy +=  normalMap * _ReflectionProperties.y; 
				fixed4 reflectionColor = tex2D(_ReflectionTex, uv1.xy);
				
				half2 uv2 = sspos.xy; uv2.xy += normalMap * _ReflectionProperties.y; 
				fixed4 refractionColor = tex2D(_RefractionTex, uv2.xy);
				
				fixed4 waterAlbedo = lerp(reflectionColor, refractionColor, fresnel);
					   waterAlbedo = lerp(waterAlbedo, waterColor, waterColor.a);
					   
				// Lighting
				norm = normalize(norm + half3(normalMap.r,normalMap.b,normalMap.g) * _ReflectionProperties.y);
				half diffFactor = max(dot(norm, lightDir), 0);
				
				half3 halfVector = (viewDir + lightDir)*0.5;
				half specFactor = pow(max(dot(norm, halfVector), 0), _LightProperties.x) * _LightProperties.y;
				
				fixed4 finalColor = (waterAlbedo*diffFactor + specFactor) *_LightColor0;

				float att = LIGHT_ATTENUATION(i);
				finalColor *= max(att, 0.25);
				
				finalColor.a = i.color.g;
				
				return finalColor;
			}
			
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
