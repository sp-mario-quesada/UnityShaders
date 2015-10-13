Shader "socialPointGLSL/sGLSL_dynPL_character" {
	Properties {
		_Overbright ("Overbright", Float) = 0.5 
		
		_SpecAmount ("Specular amount", Float) = 1.0 
		_GlossAmount ("Glossiness amount", Float) = 4.0 
		
		_AmbiAmount ("Ambient amount", Float) = 1.0 
		
		
		_EmiAmount ("Emissive amount", Float) = 1.0 
		_EmiCol ("Emissive color", Color) = (1,1,1,1)
		
		_RimAmount ("Rim amount", Float) = 1
		_RimGloss ("Rim Gloss", Float) = 3.5
		_RimCol ("Rim color", Color) = (1,1,1,1)
		
		_DF ("Diffuse map", 2D) = "white" {} 
		_MK ("Mask map", 2D) = "white" {} 
		_NM ("Normal map", 2D) = "white" {}
	}
	
	SubShader {
	
		// INI TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		
		LOD 600
		
		Tags { 	
			//- HELP: http://docs.unity3d.com/Manual/SL-SubshaderTags.html
			
			//"Queue"="Background " 	// this render queue is rendered before any others. It is used for skyboxes and the like.
			"Queue"="Geometry" 		// (default) - this is used for most objects. Opaque geometry uses this queue.
			//"Queue"="AlphaTest" 		// alpha tested geometry uses this queue.
			//"Queue"="Transparent" 	// alpha blend pixels here!
			//"Queue"="Overlay" 		// Anything rendered last should go in overlays i.e. lens flares
			
			
			//- HELP: http://docs.unity3d.com/Manual/SL-PassTags.html
			
			//"LightMode" = "Always" 			// Always rendered; no lighting is applied.
			"LightMode" = "ForwardBase"		// Used in Forward rendering, ambient, main directional light and vertex/SH lights are applied.
			//"LightMode" = "ForwardAdd"		// Used in Forward rendering; additive per-pixel lights are applied, one pass per light.
			//"LightMode" = "Pixel"				// ??
			//"LightMode" = "PrepassBase"		// deferred only...
			//"LightMode" = "PrepassFinal"		// deferred only...
			//"LightMode" = "Vertex"			// Used in Vertex Lit rendering when object is not lightmapped; all vertex lights are applied.
			//"LightMode" = "VertexLMRGBM"		// VertexLMRGBM: Used in Vertex Lit rendering when object is lightmapped; on platforms where lightmap is RGBM encoded.
			//"LightMode" = "VertexLM"			// Used in Vertex Lit rendering when object is lightmapped; on platforms where lightmap is double-LDR encoded (generally mobile platforms and old dekstop GPUs).
			//"LightMode" = "ShadowCaster"		// Renders object as shadow caster.
			//"LightMode" = "ShadowCollector"	// Gathers object’s shadows into screen-space buffer for Forward rendering path.
			
			
			//"IgnoreProjector"="True" 
		}
		
		//- HELP: http://docs.unity3d.com/Manual/SL-Blend.html
		
		//Blend SrcAlpha OneMinusSrcAlpha 	// Alpha blending
		//Blend One One 					// Additive
		//Blend OneMinusDstColor One 		// Soft Additive (screen)
		//Blend DstColor Zero 				// Multiplicative
		
		//Fog {Mode Off} // MODE: Off | Global | Linear | Exp | Exp
		
		//- HELP: http://docs.unity3d.com/Manual/SL-Pass.html
		
	    //Lighting OFF 	//Turn vertex lighting on or off
	    //ZWrite OFF	//Set depth writing mode
	    //Cull OFF 		//Back | Front | Off = two sided
		//ZTest Always  //Always = paint always front (Less | Greater | LEqual | GEqual | Equal | NotEqual | Always)
		//AlphaTest 	//(Less | Greater | LEqual | GEqual | Equal | NotEqual | Always) CutoffValue
		
		// END TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------

		Pass {
		
        GLSLPROGRAM
 		
 		#include "UnityCG.glslinc"
 		#include "Assets/External/Shaders/GLSLCommon.h"
 		
        #ifdef VERTEX // -------------------- here begins the vertex shader
 		
 		attribute vec4 Tangent;
 		//attribute vec4 Normal;

        uniform lowp vec4 		_Color;
 		uniform lowp 	float 		_AmbiAmount;
 		uniform lowp vec4 			_LightColor0;
 		
 		varying highp vec2 	v_uv0;	
 		varying highp vec3 	v_vDir; 	
 		varying highp vec3 	v_lDir;
 		varying highp vec3 	v_H;
 		varying lowp  vec3 	v_lAmbi;	 		
 		varying lowp  vec3 	v_lCol;
 		
 		GLSL_FOG_VERT_DECLARATION
 		
		void main()
		{			
			v_uv0 	= gl_MultiTexCoord0.xy;
			
			vec4 VV = gl_Vertex; 
			
			gl_Position 	= gl_ModelViewProjectionMatrix * VV;
			UNITY_TRANSFER_FOG(gl_Position);
			
			vec3 vView 		= ObjSpaceViewDir( VV );
			vec3 vLight 	= ObjSpaceLightDir( VV );
			
			v_lAmbi = gl_LightModel.ambient.rgb * 2.0 * _AmbiAmount;
			v_lCol = _LightColor0.rgb;
			
			vec3 NN 	= gl_Normal.xyz;
			vec3 TT 	= Tangent.xyz;
			vec3 BB 	= cross( NN.xyz, TT.xyz ) * Tangent.w;

			v_vDir.x = dot(TT, vView);
			v_vDir.y = dot(BB, vView);
			v_vDir.z = dot(NN, vView);
			v_vDir 		= normalize(v_vDir);
			
			v_lDir.x = dot(TT, vLight);
			v_lDir.y = dot(BB, vLight);
			v_lDir.z = dot(NN, vLight);
			v_lDir 		= normalize(v_lDir);
			
            v_H		= normalize(v_vDir + v_lDir);
		}
 
		#endif // --------------------


		#ifdef FRAGMENT // -------------------- here begins the fragment shader
		
		uniform sampler2D 	_DF;
		uniform sampler2D	_MK;
		uniform sampler2D	_NM;
		
				
		uniform mediump float 		_Overbright;
		uniform mediump float 		_GlossAmount;
		uniform lowp 	float 		_SpecAmount;
		
		uniform mediump float _EmiAmount;
		uniform lowp vec4 _EmiCol;
		
		uniform mediump float _RimAmount;
		uniform mediump float _RimGloss;
		uniform lowp vec4 _RimCol;
		
		varying highp vec2 	v_uv0;
		varying highp vec3 	v_vDir; 	
 		varying highp vec3 	v_lDir;
 		varying highp vec3 	v_H;
 		varying lowp  vec3 	v_lAmbi;	 		
 		varying lowp  vec3 	v_lCol;
		
		GLSL_FOG_FRAG_DECLARATION
		
		void main()
		{
			
			lowp vec4 DF = texture2D(_DF, v_uv0);
			lowp vec4 MK = texture2D(_MK, v_uv0);
			lowp vec4 NM = texture2D(_NM, v_uv0) * 2.0 - 1.0;
            NM.g *= -1.0; //inverted green channel.
            
			vec3 lightDir 	= normalize(v_lDir);
            vec3 viewDir 	= normalize(v_vDir);
            
            //diffuse light calculation
            float NdotL = max (0.0, dot (NM.rgb, lightDir));
            //vec3 DFL = l_ambient + (l_color * NdotL);
			//vec3 DFL = mix(v_lAmbi , v_lCol, NdotL) + (_EmiCol.rgb * MK.g * _EmiAmount * v_blink);
			vec3 DFL = v_lAmbi + (v_lCol * NdotL) + (_EmiCol.rgb * MK.g * _EmiAmount);
			
			//fresnel
			float NdotV = min (1.0, dot (NM.rgb, viewDir));
			vec3 RIM = pow((1.0 - NdotV),_RimGloss) * _RimCol.rgb * (1.0 - NdotL) * _RimAmount; 
			
            //spacular calculation
            float NdotH = max (0.0, dot (NM.rgb, normalize(v_H)));
            NdotH = pow(NdotH, abs(_GlossAmount));
            //vec3 SPE = v_lCol * NdotH * _SpecAmount * MK.r;
			vec3 SPE = DF.rgb * NdotH * _SpecAmount * MK.r;

            //result
            lowp vec3 Complete = DF.rgb * DFL * _Overbright + SPE + RIM;
			//Complete = vec3(1.0,0.5,0.0) * DFL;// + vec3(NdotH,NdotH,NdotH);
			UNITY_APPLY_FOG(Complete);
			
			gl_FragColor = vec4(Complete.rgb,DF.a); 
		}

		#endif // --------------------

		ENDGLSL
      }
	}
	
	
//================================================================================================================================================================
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//   LOW END DEVICE VERSION:
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//================================================================================================================================================================


SubShader {
	
		// INI TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------
		
		LOD 400
		
		Tags 
		{ 	
			"Queue"="Geometry" 		// (default) - this is used for most objects. Opaque geometry uses this queue.			
			"LightMode" = "Always" 			// Always rendered; no lighting is applied.
		}
		
		// END TAGS & PROPERTIES ------------------------------------------------------------------------------------------------------------------------------------

		Pass {
		
        GLSLPROGRAM
 		
 		#include "UnityCG.glslinc"
 		
        #ifdef VERTEX // -------------------- here begins the vertex shader
 		
 		attribute vec4 Tangent;
 		//attribute vec4 Normal;

        uniform lowp vec4 		_Color;
 		uniform lowp float 		_AmbiAmount;
 		uniform lowp vec4 		_LightColor0;
 		
 		varying highp vec2 	v_uv0;
 		varying lowp  vec3 	v_DFL;
 		
		void main()
		{			
			v_uv0 	= gl_MultiTexCoord0.xy;
			
			vec4 VV = gl_Vertex; 
			
			gl_Position 	= gl_ModelViewProjectionMatrix * VV;
			
			mediump vec3 lightDir  = normalize(vec3(_WorldSpaceLightPos0));
			mediump vec3 normalDir = normalize(vec3( vec4(gl_Normal, 0.0) * _World2Object));

			lowp  vec3 lAmbi = gl_LightModel.ambient.rgb * 2.0 * _AmbiAmount;
			lowp  vec3 lCol = _LightColor0.rgb;

            float NdotL = max (0.0, dot (normalDir, lightDir));
            
            v_DFL = mix(lAmbi , lCol, NdotL);
		}
 
		#endif // --------------------


		#ifdef FRAGMENT // -------------------- here begins the fragment shader
		
		uniform sampler2D 	_DF;
		uniform sampler2D	_MK;		
				
		uniform mediump float 		_Overbright;
		
		uniform lowp float _EmiAmount;
		uniform lowp vec4 _EmiCol;
		
		varying highp vec2 	v_uv0;
		varying lowp  vec3 	v_DFL;
		
		void main()
		{
			
			lowp vec4 DF = texture2D(_DF, v_uv0);
			lowp vec4 MK = texture2D(_MK, v_uv0);
            
            //diffuse light calculation
			lowp vec3 DFL = v_DFL + (_EmiCol.rgb * MK.g);
            
            //result
            lowp vec3 Complete = DF.rgb * DFL * _Overbright;
			
			gl_FragColor = vec4(Complete.rgb,DF.a); 
		}

		#endif // --------------------

		ENDGLSL
      }
	}
	
	FallBack "Diffuse"
}
