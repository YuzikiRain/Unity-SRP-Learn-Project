Shader "Custom RP/Unlit"
{
	Properties
	{
		_BaseColor("Color", Color) =  (0.5,0.5,0.5,0.5)
	}

	SubShader
	{
		Pass
		{
			HLSLPROGRAM

			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment

			#include "UnlitPass.hlsl"



			ENDHLSL
		}
	}
}