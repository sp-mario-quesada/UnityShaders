#ifndef MQM_Noise_INCLUDED
#define MQM_Noise_INCLUDED

#include "UnityCG.cginc"

// Noise methods
float hash( float n ) { return frac(sin(n)*43758.5453123); }
float noise( in float3 x )
{
    float3 p = floor(x);
    float3 f = frac(x);
    f = f*f*(3.0-2.0*f);
	
    float n = p.x + p.y*157.0 + 113.0*p.z;
    return lerp(lerp(lerp( hash(n+  0.0), hash(n+  1.0),f.x),
                   lerp( hash(n+157.0), hash(n+158.0),f.x),f.y),
               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                   lerp( hash(n+270.0), hash(n+271.0),f.x),f.y),f.z);
}

float fbm(float3 p)
{
	float f = 0.;
	
	f += 0.500000 * noise(p); p *= 2.02;
	f += 0.250000 * noise(p); p *= 2.04;
	f += 0.125000 * noise(p); p *= 2.06;
	f += 0.062500 * noise(p); p *= 2.08;
	f += 0.031250 * noise(p); p *= 3.03;
	f += 0.015625 * noise(p);
	f /= 0.984375;
		
	return f;
}

#endif // MQM_Noise_INCLUDED
