#ifndef UNITY_LIGHT_INCLUDED
#define UNITY_LIGHT_INCLUDED
#define MAX_DIRECTIONAL_LIGHT_COUNT 4

// 通过外部cpu端设置
/*
因为着色器对结构化缓冲区的支持还不够好。
它们要么根本不受支持，要么仅在片段程序中使用，要么性能比常规数组差。
好消息是数据在 CPU 和 GPU 之间传递的细节只在少数几个地方很重要，所以很容易改变。
这是使用Light结构体的另一个好处。
*/
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

DirectionalShadowData GetDirectionalShadowData(int lightIndex, ShadowData shadowData)
{
	// 这里的 _DirectionalLightShadowData 并不是DirectionalShadow类型
	// 只是装了对应的数据到各分量上，所以这里还需要重新构造下
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
	data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;

	return data;
}

Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData)
{
	Light light;
	light.color = _DirectionalLightColors[index];
	light.direction = _DirectionalLightDirections[index];
	DirectionalShadowData directionalShadowData = GetDirectionalShadowData(index, shadowData);
	light.attenuation = GetDirectionalShadowAttenuation(directionalShadowData, surfaceWS);
	//light.attenuation = shadowData.cascadeIndex * 0.25;
	return light;
}

#endif