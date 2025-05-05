#ifndef LERPTHREE_HLSL
#define LERPTHREE_HLSL

void LerpThree_float(float4 a, float4 b, float4 c, float t, out float4 Out)
{
    float halfT = saturate(t * 2.0);
    float secondT = saturate((t - 0.5) * 2.0);
    float4 ab = lerp(a, b, halfT);
    float4 bc = lerp(b, c, secondT);
    Out = (t < 0.5) ? ab : bc;
}

#endif
