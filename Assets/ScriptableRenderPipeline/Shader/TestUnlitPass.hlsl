// 这个文件写Vertex Fragment具体的方法
#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"



TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
// support per-instance material data
CBUFFER_START(UnityPerMaterial)
	float _Test2;
	float4 _BaseMap_ST;
			//float4 _BaseColor;
CBUFFER_END

//CBUFFER_START(UnityPerDraw)
//	float4x4 unity_ObjectToWorld;
//	float4x4 unity_WorldToObject;
//	float4 unity_LODFade;
//	float4 unity_WorldTransformParams;
//CBUFFER_END



struct Attributes
{
	float3 positionOS : POSITION;
	float2 baseUV : TEXCOORD0;
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float2 baseUV : VAR_BASE_UV;
};

Varyings UnlitPassVertex(Attributes input)
{
	Varyings output;

	// TransformObjectToWorld 其实就是 mul(UNITY_MATRIX_M, targetMatrix)
	// UNITY_MATRIX_M是一个 uniform value，每次绘制（渲染帧）时由GPU设置该值（这次绘制过程中不会改变）
	float3 positionWS = TransformObjectToWorld(input.positionOS);
	output.positionCS = TransformWorldToHClip(positionWS);

	float4 baseST = _BaseMap_ST;
	output.baseUV = input.baseUV * baseST.xy + baseST.zw;
	//output.baseUV =input.baseUV;
	return output;
}

// SV_TARGET 表明Render Target 使用 默认系统值（default system value）
float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
	float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
	float4 baseColor = 1 ;
	float4 finalColor = baseMap * baseColor;
	#if defined(_CLIPPING)
		clip(finalColor.a -  _Cutoff);
	#endif
	return finalColor;
}

#endif