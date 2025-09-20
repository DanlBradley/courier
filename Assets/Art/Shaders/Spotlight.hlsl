#ifndef FLAT_SPOTLIGHT_INCLUDED
#define FLAT_SPOTLIGHT_INCLUDED

void FlatSpotlight_float(float3 WorldPos, float3 LightPos, float3 LightDir, float SpotAngle, float Range, out float LightMask)
{
    float3 lightToFrag = WorldPos - LightPos;
    float distance = length(lightToFrag);
    float3 lightToFragDir = lightToFrag / max(distance, 0.001);
    
    float cosAngle = dot(normalize(LightDir), lightToFragDir);
    float angle = acos(clamp(cosAngle, -1.0, 1.0));
    
    // Use inline calculation instead of PI constant
    float halfAngleRad = (SpotAngle * 0.5) * 0.017453292;
    
    float spotMask = step(angle, halfAngleRad);
    float rangeMask = step(distance, Range);
    
    LightMask = spotMask * rangeMask;
}

#endif