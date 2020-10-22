
//void Unity_SampleGradient_float(Gradient gradient, float Time, out float4 Out)
//{
//    float3 color = gradient.colors[0].rgb;
//    [unroll]
//    for (int c = 1; c < 8; c++)
//    {
//        float colorPos = saturate((Time - gradient.colors[c - 1].w) / (gradient.colors[c].w - gradient.colors[c - 1].w)) * step(c, gradient.colorsLength - 1);
//        color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
//    }
//#ifndef UNITY_COLORSPACE_GAMMA
//    color = SRGBToLinear(color);
//#endif
//    float alpha = gradient.alphas[0].x;
//    [unroll]
//    for (int a = 1; a < 8; a++)
//    {
//        float alphaPos = saturate((Time - gradient.alphas[a - 1].y) / (gradient.alphas[a].y - gradient.alphas[a - 1].y)) * step(a, gradient.alphasLength - 1);
//        alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
//    }
//    Out = float4(color, alpha);
//}
void Unity_SampleGradient_half(Gradient gradient, half Time, out half4 Out)
{
    half3 color = gradient.colors[0].rgb;
    [unroll]
    for (int c = 1; c < 8; c++)
    {
        half colorPos = saturate((Time - gradient.colors[c - 1].w) / (gradient.colors[c].w - gradient.colors[c - 1].w)) * step(c, gradient.colorsLength - 1);
        color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif
    float alpha = gradient.alphas[0].x;
    [unroll]
    for (int a = 1; a < 8; a++)
    {
        half alphaPos = saturate((Time - gradient.alphas[a - 1].y) / (gradient.alphas[a].y - gradient.alphas[a - 1].y)) * step(a, gradient.alphasLength - 1);
        alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
    }
    Out = half4(color, alpha);
}

void Displacement_half(half3 _position, half3 _normal, half3 _direction, half _lod,
    Texture2D _main, SamplerState _mainSampler, half2 _mainUv, half2 _mainRes,
    Texture2D _mask, SamplerState _maskSampler, half2 _maskUv,
    half _depthHeight, Gradient _depthInfluence,
    half _detailStrength, Gradient _detailInfluence,
    out half3 position_, out half3 normal_, out half rawHeight_, out half depth_, out half detail_, out half mask_) {
    half height_main = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv, _lod).r;
    mask_ = SAMPLE_TEXTURE2D_LOD(_mask, _maskSampler, _maskUv, _lod).r;
    half4 depth = 0;
    Unity_SampleGradient_half(_depthInfluence, height_main, depth);
    depth_ = 1-(depth.r * mask_);
    half4 detail = 0;
    Unity_SampleGradient_half(_detailInfluence, height_main, detail);
    detail_ = detail.r;
    height_main = depth.r;
    height_main += detail.r * _detailStrength;
    height_main = -height_main * mask_;
    rawHeight_ = -height_main - 1;
    half2 texel = half2(1 / _mainRes.r, 1 / _mainRes.g);
    half4 h;

    h[0] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(0, -1 * texel.g), _lod).r;
    Unity_SampleGradient_half(_depthInfluence, h[0], depth);
    Unity_SampleGradient_half(_detailInfluence, h[0], detail);
    h[0] = depth.r;
    h[0] += detail.r * _detailStrength;
    h[0] = -h[0] * (-SAMPLE_TEXTURE2D_LOD(_mask, _maskSampler, _maskUv + float2(0, -1 * texel.g), _lod).r);
    h[0] *= height_main;

    h[1] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(-1 * texel.r, 0), _lod).r;
    Unity_SampleGradient_half(_depthInfluence, h[1], depth);
    Unity_SampleGradient_half(_detailInfluence, h[1], detail);
    h[1] = depth.r;
    h[1] += detail.r * _detailStrength;
    h[1] = -h[1] * (-SAMPLE_TEXTURE2D_LOD(_mask, _maskSampler, _maskUv + float2(-1 * texel.r, 0), _lod).r);
    h[1] *= height_main;

    h[2] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(1 * texel.r, 0), _lod).r;
    Unity_SampleGradient_half(_depthInfluence, h[2], depth);
    Unity_SampleGradient_half(_detailInfluence, h[2], detail);
    h[2] = depth.r;
    h[2] += detail.r * _detailStrength;
    h[2] = -h[2] * (-SAMPLE_TEXTURE2D_LOD(_mask, _maskSampler, _maskUv + float2(1 * texel.r, 0), _lod).r);
    h[2] *= height_main;

    h[3] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(0, 1 * texel.g), _lod).r;
    Unity_SampleGradient_half(_depthInfluence, h[3], depth);
    Unity_SampleGradient_half(_detailInfluence, h[3], detail);
    h[3] = depth.r;
    h[3] += detail.r * _detailStrength;
    h[3] = -h[3] * (-SAMPLE_TEXTURE2D_LOD(_mask, _maskSampler, _maskUv + float2(0, 1 * texel.g), _lod).r);
    h[3] *= height_main;

    //h[0] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(0  * texel.r, -1 * texel.g), _lod).r * height_main;
    //h[1] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(-1 * texel.r, 0  * texel.g), _lod).r * height_main;
    //h[2] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(1  * texel.r, 0  * texel.g), _lod).r * height_main;
    //h[3] = SAMPLE_TEXTURE2D_LOD(_main, _mainSampler, _mainUv + float2(0  * texel.r, 1  * texel.g), _lod).r * height_main;

    normal_.z = -(h[0] - h[3]);
    normal_.x = -(h[1] - h[2]);
    normal_.y = 2 * texel;
    normal_ = normalize(normal_);

    half3 dir = _normal * _direction;
    half3 direlse = _normal * (1 - _direction);

    dir = dir.r + _depthHeight + rawHeight_ * _depthHeight;
    dir *= _direction;
    dir += direlse;

    position_ = _position + dir;
    rawHeight_ = rawHeight_;
}
void Test_half(
    Texture2D _main, SamplerState _mainSampler, half2 _mainUv, out half3 color_) {
    color_ = SAMPLE_TEXTURE2D(_main, _mainSampler, _mainUv);
}