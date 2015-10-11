#ifndef INC_POST_PROCESS_HELPER_CGINC
#define INC_POST_PROCESS_HELPER_CGINC

#define BLUR_KERNEL_SIZE 9
#define BLUR1D_KERNEL_SIZE 7

void GetBlurData1D(float width, float height, out float kernel[BLUR1D_KERNEL_SIZE], out float2 offfset[BLUR1D_KERNEL_SIZE], half2 dir)
{
	kernel[0] = 0.0205;
	kernel[1] = 0.0855;
	kernel[2] = 0.232;
	kernel[3] = 0.324;
	kernel[4] = 0.232;
	kernel[5] = 0.0855;
	kernel[6] = 0.0205;
	
	dir.x /= width;
	dir.y /= height;
	
	offfset[0] = 3.0*float2(-1.0, -1.0) * dir;
	offfset[1] = 2.0*float2(-1.0, -1.0) * dir;
	offfset[2] = 1.0*float2(-1.0, -1.0) * dir;
	offfset[3] = float2(0.0, 0.0);
	offfset[4] = 1.0*float2(1.0, 1.0) * dir;
	offfset[5] = 2.0*float2(1.0, 1.0) * dir;
	offfset[6] = 3.0*float2(1.0, 1.0) * dir;
}

void GetBlurData(float width, float height, out float kernel[BLUR_KERNEL_SIZE], out float2 offfset[BLUR_KERNEL_SIZE])
{
	kernel[0] = 1.0/16.0;
	kernel[1] = 2.0/16.0;
	kernel[2] = 1.0/16.0;
	kernel[3] = 2.0/16.0;
	kernel[4] = 4.0/16.0;
	kernel[5] = 2.0/16.0;
	kernel[6] = 1.0/16.0;
	kernel[7] = 2.0/16.0;
	kernel[8] = 1.0/16.0;
		
	float step_w = 1.0/width;
	float step_h = 1.0/height;
	
	offfset[0] = float2(-step_w, -step_h);
	offfset[1] = float2(0.0, -step_h);
	offfset[2] = float2(step_w, -step_h);
	offfset[3] = float2(-step_w, 0.0);
	offfset[4] = float2(0.0, 0.0);
	offfset[5] = float2(step_w, 0.0);
	offfset[6] = float2(-step_w, step_h);
	offfset[7] = float2(0.0, step_h);
	offfset[8] = float2(step_w, step_h);
}

half2 GetHalfPixel(float width, float height)
{
	return half2(50.25/width, 0.25/height);
	//return half2(0, 0);
}

#endif
