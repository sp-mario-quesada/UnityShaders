Shader "SocialPoint/SocialPointCascade"
{
	//  Wave Height are scaled by VertexColor.r
	//  Alpha by VertexColor.g
	//  Color by VertexColor.b
	Properties
	{
		_Cube ("Cube", Cube) = "white" {}
		_NormalMap ("NormalMap", 2D) = "black" {}
		_NormalMapAnimation("x:_NormalMapAnimation.U, y:_NormalMapAnimation.V", Vector) = (0.5, 0.5, 0, 0)
		
		_ReflectionProperties ("x:ReflPower(1-100),y:ReflDist(0-1),z:RefractDist(0-1),w:Nothing", Vector) = (1,0.01,0.01,0)
		_LightProperties ("x:specPower(1-100),y:SpecInt(0-4)", Vector) = (2,1.0,1.0,1)
		
		_UVScale ("UVScale", Vector) = (1,1,1,1)
		_WaveScale ("x:WavePhaseScale.x,y:WavePhaseScale.y,z:WaveHightScale,w:WaveSpeed", Vector) = (30,30,1,1)
		
		_WaterTex1 ("_WaterTex1", 2D) = "white" {}
		_WaterTex2 ("_WaterTex2", 2D) = "white" {}
		
		_MainWaterColor ("_MainWaterColor", Color) = (0.7,8,0.7,0)
		_FresnelColor ("_FresnelColor", Color) = (0.7,8,0.7,0)
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
				float3 normVS : TEXCOORD0;
				float4 uvNormalMap : TEXCOORD1;
				float4 uvWaterTex : TEXCOORD2;
				float3 wpos : TEXCOORD3;
				float4 sspos : TEXCOORD4;
				LIGHTING_COORDS(5,6)
			};
			
			sampler2D _WaterTex1;
			float4 _WaterTex1_ST;
			sampler2D _WaterTex2;
			float4 _WaterTex2_ST;
			
			sampler2D _NormalMap;
			float4 _NormalMap_ST;
			half4 _NormalMapAnimation;
			
			samplerCUBE _Cube;
			half4 _ReflectionProperties;
			half4 _LightProperties;
			
			half4 _UVScale;
			half4 _WaveScale;
			
			fixed4 _MainWaterColor;
			fixed4 _FresnelColor;
			
			sampler2D _ReflectionTex;
			sampler2D _RefractionTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.color = v.color;
				
				float4 vertex = v.vertex;
				
				o.wpos = mul(_Object2World, float4(vertex.xyz, 1));
				float3 pivotPos = float3(_Object2World[0][1], _Object2World[0][1], _Object2World[0][2]);
				float3 localPos = o.wpos - pivotPos;
				float enlargeFactor = smoothstep(-1.5, 6, -localPos.y);
//				o.wpos.y += ((1+sin(_Time.y))*0.5) * 10 * enlargeFactor;

				o.wpos.xz += sin(length(o.wpos.xy)*100 + _Time.y*15) * 0.15 * enlargeFactor;
				
				half4 uv = v.uv.xyxy;
				uv.xy *= _UVScale.xy;
				uv.zw *= _UVScale.zw;
				half2 uvScaleHalf = (_UVScale.xy + _UVScale.zw)*0.5;
				
				o.pos = mul(UNITY_MATRIX_VP, float4(o.wpos.xyz, 1));
				o.norm = normalize( mul((float3x3) _Object2World, v.normal.xyz) );
				o.normVS = normalize( mul((float3x3) UNITY_MATRIX_MV, v.normal.xyz) );
				o.sspos = o.pos;
				
				o.uvNormalMap.xy = TRANSFORM_TEX(uv.xy, _NormalMap).xy + _NormalMapAnimation.xy * _Time.x;
				o.uvNormalMap.zw = TRANSFORM_TEX(uv.zw, _NormalMap).xy + _NormalMapAnimation.zw * _Time.x;
				
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
				half3 reflVector = normalize(-viewDir + 2.0*dot(viewDir, norm)*norm);
				
				// Fresnel
				
				half fresnel = max(dot(viewDir, norm), 0);
				half rim = 1-fresnel;
				
				// Texture Fetch
				half3 normalMap1 = UnpackNormal(tex2D(_NormalMap, i.uvNormalMap.xy)).xyz;
				half3 normalMap2 = UnpackNormal(tex2D(_NormalMap, i.uvNormalMap.zw)).xyz;
				half3 normalMap = (normalMap1 + normalMap2) * 0.5;
				
				fixed4 waterColor1 = tex2D(_WaterTex1, i.normVS.xy + normalMap1.xy * 0.5);
				fixed4 waterColor2 = tex2D(_WaterTex2, i.normVS.xy + normalMap2.xy * 0.5);
				fixed4 waterColor = lerp(waterColor1, waterColor2, normalMap2.b) * _MainWaterColor;
				//fixed4 waterColor = (waterColor1 + waterColor2 * normalMap2.b) * _MainWaterColor;
					   waterColor = lerp(waterColor, waterColor+_FresnelColor, fresnel);
				
				half3 uv1 = reflVector; uv1+=  normalMap * _ReflectionProperties.y; 
				fixed4 reflectionColor = texCUBE(_Cube, uv1);
				
				fixed4 waterAlbedo = lerp(waterColor, reflectionColor, 0);
					   
				// Lighting
				norm = normalize(norm + half3(normalMap.r,normalMap.b,normalMap.g) * _ReflectionProperties.y);
				half diffFactor = max(dot(norm, lightDir), 0);
				
				half3 halfVector = (viewDir + lightDir)*0.5;
				half specFactor = pow(max(dot(norm, halfVector), 0), _LightProperties.x) * _LightProperties.y;
				
				fixed4 finalColor = (waterAlbedo*diffFactor + specFactor) *_LightColor0;

				float att = LIGHT_ATTENUATION(i);
				finalColor *= max(att, 0.25);
				
				finalColor.a = 0.99+0.01*pow(fresnel, 7) * i.color.g;
				
				return finalColor;
			}
			
			ENDCG
		}
	}
	
	//FallBack "Diffuse"
}
