void CalculateWorldOcclusion_float(
    float3 PixelWorldPos,
    float3 PlayerWorldPos,
    float3 CameraWorldPos,
    float3 WorldNormal,
    out float OutMask)
{
    float radius = 4.0;
    float depthBias = 2.0; // Offset ahead of player to avoid wall-hacking
    float distToPixel = distance(CameraWorldPos, PixelWorldPos);
    float distToPlayer = distance(CameraWorldPos, PlayerWorldPos);
    float playerToPixel = distance(PixelWorldPos, PlayerWorldPos);
    float distFromPlayer = distance(PixelWorldPos.xz, PlayerWorldPos.xz);
    
    
    
    float circleMask = 1.0 - smoothstep(radius - 0.5, radius, distFromPlayer); // Smooth, so dictates alpha
    float depthMask = step(distToPixel, distToPlayer - depthBias);
    float heightMask = step(PlayerWorldPos.y, PixelWorldPos.y);
    float northMask = step(0.5, -WorldNormal.z) * step(abs(WorldNormal.x), abs(WorldNormal.z));

    float occlusion = circleMask * depthMask * heightMask * northMask;


    OutMask = 1.0 - occlusion;
}
