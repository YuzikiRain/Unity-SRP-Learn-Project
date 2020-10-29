#ifndef UNITY_LIGHT_INCLUDED
#define UNITY_LIGHT_INCLUDED
#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
	float3 color;
	float3 direction;
	float attenuation;
};

int GetDirectionalLightCount(){ return _DirectionalLightCount; }

DirectionalShadowData GetDirectionalShadowData(int lightIndex)
{
	// 这里的 _DirectionalLightShadowData 并不是DirectionalShadow类型
	// 只是装了对应的数据到各分量上，所以这里还需要重新构造下
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[lightIndex].x;
	data.tileIndex = _DirectionalLightShadowData[lightIndex].y;

	return data;
}

Light GetDirectionalLight(int index, Surface surfaceWS)
{
	Light light;
	light.color = _DirectionalLightColors[index];
	light.direction = _DirectionalLightDirections[index];
	DirectionalShadowData shadowData = GetDirectionalShadowData(index);
	light.attenuation = GetDirectionalShadowAttenuation(shadowData, surfaceWS);
	return light;
}

#endif