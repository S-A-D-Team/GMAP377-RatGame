#ifndef EDGESTR_HLSL
#define EDGESTR_HLSL

void EdgeStr_float(float3 normalWS, float edgeIntensity)
{
    float3 dx = ddx(normalWS);
    float3 dy = ddy(normalWS);
    float edge = length(dx) + length(dy);
    return saturate(edge * edgeIntensity);
}

#endif
