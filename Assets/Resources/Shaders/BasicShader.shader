Shader "SocialPoint/Basic"
{
	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
		}
		Pass
		{
			LOD 100
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct vertexdata
			{
				float4 pos : POSITION;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert(vertexdata i)
			{
				v2f o = (v2f) 0;
				o.pos = mul(UNITY_MATRIX_MVP, i.pos);
				
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				return fixed4(0,0,0,0);
			}
			
			ENDCG
		}
	}
}