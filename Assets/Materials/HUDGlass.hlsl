void HUDGlass_float(
    float3 NormalOS,
    float3 PositionOS,
    out float3 OutEmission,
    out float OutAlpha)
{
    float rimPower = 6.0; // Increase to make the rim thinner
    float3 glassTint = float3(0.2, 0.6, 0.8); // Normalized color

    float radialDist = length(PositionOS.xy);
    float fresnel = pow(saturate(radialDist), rimPower);

    // Subtract a threshold so the interior goes completely to 0 alpha
    float alpha = saturate((fresnel - 0.4) * 3.0);

    OutEmission = glassTint * fresnel;
    OutAlpha = alpha;
}
