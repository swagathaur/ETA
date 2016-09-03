Shader " MyShaders/Cel/WithOutline"
{
	//Variables for unity editor
	Properties
	{
		_MainTexture("Main Colour (RGB)", 2D) = "purple" {}
		_Color("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Pass
		{
			//Start Shader Code
			CGPROGRAM

			//names for functions
			#pragma vertex vertexFunction
			#pragma fragment fragmentFunction

			#include "UnityCG.cginc"

			//Vertext Function, builds object
			struct appdata
			{
				float4 vertexPointsFromMesh : POSITION;
				float2 uv :TEXCOORD0;

			};

			//Fragment Function, draws and colours



			//End Shader Code
			ENDCG
		}

	}

}