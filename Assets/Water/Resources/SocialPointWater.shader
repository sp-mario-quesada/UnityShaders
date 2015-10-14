Shader "SocialPoint/SocialPointWater"
{
	//  Vertex Movement Height are scaled by VertexColor.r
	//  Alpha by VertexColor.g
	Properties
	{
		_NormalMapAnimation("x:_NormalMapAnimation.U, y:_NormalMapAnimation.V", Vector) = (0.5, 0.5, 0, 0)
		
		_Cube ("Cube", Cube) = "white" {}
		_NormalMap ("NormalMap", 2D) = "black" {}
		_ReflectionProperties ("x:ReflPower(1-100),y:ReflDist(0-1),z:RefractDist(0-1),w:NormalBlendFactor", Vector) = (1,0.01,0.01, 1.5)
		_LightProperties ("x:specPower(1-100),y:SpecInt(0-4)", Vector) = (2,1.0,1.0,1)
		
		_UVScale ("UVScale", Vector) = (1,1,1,1)
		_WaveScale ("x:WavePhaseScale.x,y:WavePhaseScale.y,z:WaveHightScale,w:WaveSpeed", Vector) = (30,30,1,1)
		
		_HorizonColor ("_HorizonColor", Color) = (0,0,0,0)
		
		_ReflectionTex ("_ReflectionTex", 2D) = "white" {}
		_RefractionTex ("_RefractionTex", 2D) = "white" {}
		
		_WaterwallPivot ("_WaterwallPivot", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags {
		 "Queue"="Geometry+100" 
		 "RenderType"="Opaque" 
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

			sampler2D _NormalMap;
			float4 _NormalMap_ST;
			half4 _NormalMapAnimation;
			
			samplerCUBE _Cube;
			half4 _ReflectionProperties;
			half4 _LightProperties;
			
			half4 _UVScale;
			half4 _WaveScale;
			
			fixed4 _HorizonColor;
			
			half4 _WaterwallPivot;
			
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
				
				half2 collisionDir = o.wpos.xz - _WaterwallPivot.xz;
				half collisionDist = length(collisionDir);
				
				half waveSpeed = lerp(-10, -10, smoothstep(0, 50, collisionDist));
				half waveScale = lerp(1.5, 0, smoothstep(0, 50, collisionDist));
				o.wpos.y += lerp( sin(o.wpos.x*uvScaleHalf.x*_WaveScale.x + _NormalMapAnimation.xy * _WaveScale.w * _Time.y) * sin(o.wpos.z*uvScaleHalf.y*_WaveScale.y + _NormalMapAnimation.y * _WaveScale.w * _Time.y) *_WaveScale.z * v.color.r,
								  sin(collisionDist*1.0 + _Time.y*waveSpeed) *_WaveScale.z * v.color.r * waveScale,
								     step(0.5, _WaterwallPivot.w));
				//o.wpos.y += 0.5 * sin(o.wpos.x*uvScaleHalf.x*_WaveScale.x + _NormalMapAnimation.xy * _WaveScale.w * _Time.y) * sin(o.wpos.z*uvScaleHalf.y*_WaveScale.y + _NormalMapAnimation.y * _WaveScale.w * _Time.y) *_WaveScale.z * v.color.r;
				
				o.pos = mul(UNITY_MATRIX_VP, float4(o.wpos.xyz, 1));
				o.norm = normalize( mul((float3x3) _Object2World, v.normal.xyz) );
				o.sspos = o.pos;
				
				collisionDir = normalize(collisionDir);
				o.uvNormalMap.xy = uv.xy + collisionDir.xy * _Time.x;
				o.uvNormalMap.zw = uv.zw + collisionDir.xy * _Time.x;
				
				o.uvNormalMap.xy = uv.xy + _NormalMapAnimation.xy * _Time.x;
				o.uvNormalMap.zw = uv.zw + _NormalMapAnimation.zw * _Time.x;
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 sspos = ((i.sspos.xy/i.sspos.w)+1)*0.5;
				//sspos.y = 1 - sspos.y;
			
				// Vectors
				half3 norm = normalize(i.norm);
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wpos.xyz);
				half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
//				half3 reflVector = normalize(-viewDir + 2.0*dot(viewDir, norm)*norm);
				
				// Texture Fetch
				half3 normalMap1 = UnpackNormal(tex2D(_NormalMap, i.uvNormalMap.xy)).xyz;
				half3 normalMap2 = UnpackNormal(tex2D(_NormalMap, i.uvNormalMap.zw)).xyz;
				half3 normalMap = (normalMap1 + normalMap2) * _ReflectionProperties.w;
				
				norm = normalize(norm + half3(normalMap.r,normalMap.b,normalMap.g) * _ReflectionProperties.y);
				
				// Fresnel
				half fresnel = max(dot(viewDir, norm), 0);
				half rim = 1-fresnel;
				
				half2 uv1 = sspos.xy; uv1.xy +=  normalize(normalMap) * _ReflectionProperties.y; 
				fixed4 reflectionColor = tex2D(_ReflectionTex, uv1.xy);
				
				half2 uv2 = sspos.xy; uv2.xy += normalize(normalMap) * _ReflectionProperties.z; 
				fixed4 refractionColor = tex2D(_RefractionTex, uv2.xy);
				
				fixed4 waterAlbedo = lerp(refractionColor, reflectionColor, rim);
					  waterAlbedo = lerp(waterAlbedo, lerp(waterAlbedo, _HorizonColor, 0.5), pow(rim, 4));
				
				half diffFactor = 1;//max(dot(norm, lightDir), 0);
				
				half3 halfVector = (viewDir + lightDir)*0.5;
				half specFactor = pow(max(dot(norm, halfVector), 0), _LightProperties.x) * _LightProperties.y;
				
				fixed4 finalColor = (waterAlbedo*diffFactor + specFactor) *_LightColor0* lerp(0.3, 1, smoothstep(0, 0.25, lightDir.y));

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
