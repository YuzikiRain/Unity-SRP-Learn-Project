#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
// View-Projection Matrix
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

//// 模型空间到世界空间的变换矩阵
//float3 TransformObjectToWorld(float3 positionOS)
//{
//	return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
//}

//// 世界空间到裁剪（投影）空间的变换矩阵
//float4 TransformWorldToHClip(float3 positionWS)
//{
//	return mul(unity_MatrixVP, float4(positionWS, 1.0));
//}

#endif