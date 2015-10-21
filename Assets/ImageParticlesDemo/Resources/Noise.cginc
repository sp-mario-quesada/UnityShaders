
#ifndef NOISE_CG_INCLUDED
#define NOISE_CG_INCLUDED

float hash1(float2 p)
{
	return frac( sin(length(p)) * 43758.5453 );
}

float hash1(float x)
{
	return frac( sin(x) * 43758.5453 );
}

float noise(float2 p)
{
	float2 i = floor(p);
	float2 f = frac(p);
	f *= f * (3.0-2.0*f);
	
	return lerp(
		lerp(hash1(i + float2(0,0)), hash1(i + float2(1, 0)), f.x),
		lerp(hash1(i + float2(0,1)), hash1(i + float2(1, 1)), f.x),
		f.y );
}

float fbm2(float2 p)
{
	float f = 0.0;
	
	f += 0.50000 * noise(p); p*= 2.01;
	f += 0.25000 * noise(p); p*= 2.03;
	f += 0.12500 * noise(p); p*= 2.06;
	f += 0.06250 * noise(p); p*= 3.01;
	f += 0.03125 * noise(p); p*= 3.03;
	
	f /= 0.96875;
	
	return f;
}
#endif
