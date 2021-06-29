// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "MyShaders/Cel/WithOutline"
{
	//Variables for unity editor
	Properties
	{
		_MainTex("Main Texture (RGB)", 2D) = "purple" {}
		_Color("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "LightMode" = "ForwardBase" }
		Pass
		{
			Name "BASE"
			//Start Shader Code
			CGPROGRAM

			//names for functions
			#pragma vertex vertexFunction
			#pragma fragment fragmentFunction

			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv :TEXCOORD0;
			};
			struct fragmentInput
			{
				float4 position : SV_POSITION;
				float4 color : COLOR0;
				float2 uv :TEXCOORD0;
			};

			float4 _Color;
			float4 _MainTex_ST;
			sampler2D _MainTex;
			uniform float4 _LightColor0;

			fragmentInput vertexFunction(vertexInput Input)
			{
				fragmentInput Output;
				Output.position = UnityObjectToClipPos(Input.vertex);
				Output.uv = TRANSFORM_TEX(Input.uv, _MainTex);

				float3 normalDirection = normalize( mul( float4(Input.normal, 1.0f), unity_WorldToObject).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 NdotL = dot( normalDirection, lightDirection);

				NdotL = 1 + clamp(floor(NdotL), -1, 0);

				Output.color = float4(NdotL, 1);
				return Output;
			}

			half4 fragmentFunction(fragmentInput Input) : COLOR
			{				

				half4 endColor = tex2D(_MainTex, Input.uv) * Input.color;

				return endColor;
			}



			//End Shader Code
			ENDCG
		}
		UsePass "Toon/Basic Outline/OUTLINE"

	}

}