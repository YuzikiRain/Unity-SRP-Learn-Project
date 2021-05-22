#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

//由于地图集不是常规纹理，因此我们可以通过TEXTURE2D_SHADOW宏对其进行定义，以使其清晰可见，即使它对我们支持的平台没有影响。
//我们将使用一个特殊的SAMPLER_CMP宏来定义采样器状态，因为这确实定义了一种不同的方式来采样阴影贴图，因为常规的双线性过滤对于深度数据没有意义。
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
	int _CascadeCount;
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
	float _ShadowDistance;
CBUFFER_END

struct DirectionalShadowData
{
	float strength;
	int tileIndex;
};

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}


float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS)
{
	if (data.strength <= 0) { return 1.0; }
	float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex], float4(surfaceWS.position, 1.0)).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);

	return lerp(1.0, shadow, data.strength);
}

struct ShadowData
{
	int cascadeIndex;
	float strength;
};

float FadedShadowStrength(float distance, float scale, float fade)
{
	return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surfaceWS)
{
	ShadowData data;
	data.strength = surfaceWS.depth < _ShadowDistance ? 1.0 : 0.0;
	int i;
	for (i = 0; i < _CascadeCount; i++)
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
		// 从大到小开始找到刚好比cullingSphere小一点的球对应的 index
		if (distanceSqr < sphere.w) { break; }
	}
	// 没有找到合适的级联
	if (i == _CascadeCount) { data.strength = 0.0; }
	data.cascadeIndex = i;
	return data;
}

#endif