void CalculateWallOcclusion_float(
    float2 ScreenUV,
    float2 PlayerScreenUV,
    float Aspect,
    out float OutMask
)
{
    float2 delta = ScreenUV - PlayerScreenUV;

    delta.x *= Aspect; // restore circle
    delta.y *= 5; // make it vertically squashed

    float dist = length(delta);

    float radius = 0.3;
    float falloff = 0.05;

    OutMask = saturate((dist - radius) / falloff);
}