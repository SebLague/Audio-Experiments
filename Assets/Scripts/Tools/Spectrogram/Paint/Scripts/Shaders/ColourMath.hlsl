// From https://www.shadertoy.com/view/MsS3Wc
float3 HSVtoRGB(float3 c)
{
    float3 rgb = saturate(abs(fmod(c.x * 6 + float3(0, 4, 2), 6) - 3) - 1);
	//rgb = rgb * rgb * (3 - 2 * rgb); // cubic smoothing	
	return c.z * lerp(1, rgb, c.y);
}

// From http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
float3 RGBtoHSV(float3 c)
{
    float4 K = float4(0, -1.0 / 3, 2.0 / 3, -1);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 0.0000000001;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// Thanks to https://www.shadertoy.com/view/4tXcWr
float3 linearToSRGB(float3 linearRGB)
{
	bool3 cutoff = linearRGB < 0.0031308;
	float3 higher = 1.055 * pow(saturate(linearRGB), 1.0/2.4) - 0.055;
	float3 lower = linearRGB * 12.92;

	return lerp(higher, lower, cutoff);
}

// Thanks to https://www.shadertoy.com/view/4tXcWr
float3 sRGBToLinear(float3 sRGB)
{
	bool3 cutoff = sRGB < 0.04045;
	float3 higher = pow((saturate(sRGB) + 0.055) / 1.055, 2.4);
	float3 lower = sRGB/12.92;

	return lerp(higher, lower, cutoff);
}

float3 blendColour(float3 a, float3 b, float t) {
	return linearToSRGB(lerp(sRGBToLinear(a), sRGBToLinear(b), t));
}