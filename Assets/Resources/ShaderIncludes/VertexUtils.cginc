#ifndef INC_VERTEX_UTILS_CGINC
#define INC_VERTEX_UTILS_CGINC

float3 Spherify(float3 wpos, float sphereIntensity)
{
	float3 camDir = (wpos.xyz - _WorldSpaceCameraPos);
	wpos.y -= length(camDir.xz) * sphereIntensity;
	return wpos;
}

#endif
