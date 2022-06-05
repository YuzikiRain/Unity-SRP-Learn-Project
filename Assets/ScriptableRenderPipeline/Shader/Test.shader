Shader "Custom RP/TestUnlit"
{
	Properties
	{
		_BaseMap("Texture", 2D) = "white" {}
		//_BaseColor("Color", Color) =  (1, 1, 1, 1)
		_Test1("Test1", Vector) = (1,22,3,4)
		_Test2("Test2", Float) = 1
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		[Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
	}

	SubShader
	{


		Pass
		{

			// 不需要设置tag也可以正常绘制
			//Tags {"LightMode" = "SRPDefaultUnlit"}
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			// 用 HLSLPROGRAM 和 ENDHLSL 关键字包裹hlsl代码
			HLSLPROGRAM

			#pragma target 3.5

			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			// #pragma vertex 顶点着色器名称
			#pragma vertex UnlitPassVertex
			// #pragma fragment 片元着色器名称
			#pragma fragment UnlitPassFragment

			#include "TestUnlitPass.hlsl"

			ENDHLSL
		}
	}

	CustomEditor "CustomShaderGUI"
}