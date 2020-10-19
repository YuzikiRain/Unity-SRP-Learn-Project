Shader "SRPStudy/UnlitTexture"
{
	Properties
	{
		_Color("Color Tint", Color) = (0.5,0.5,0.5)
		_MainTex("MainTex",2D) = "white"{}




	}

	HLSLINCLUDE
	#include "UnityCG.cginc"

	uniform float4 _Color;
	sampler2D _MainTex;

	struct a2v
	{
		float4 position : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f vert(a2v v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.position = UnityObjectToClipPos(v.position);
		o.uv = v.uv;
		return o;
	}

	half4 frag(v2f v) : SV_Target
	{
		half4 fragColor = half4(_Color.rgb,1.0) * tex2D(_MainTex, v.uv);
		return fragColor;
	}

	ENDHLSL

	SubShader
	{		
		Tags{ "Queue" = "Geometry" }
		LOD 100
		Pass
		{
			//注意这里,默认是没写光照类型的,自定义管线要求必须写,渲染脚本中会调用,否则无法渲染
			//这也是为啥新建一个默认unlitshader,无法被渲染的原因
			Tags{ "LightMode" = "Always" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
	}
}