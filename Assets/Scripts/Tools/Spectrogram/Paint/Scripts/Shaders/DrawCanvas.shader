Shader "Custom/DrawCanvas"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_StrokeTex ("Stroke Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "./ColourMath.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _StrokeTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 canvasCol = tex2D(_MainTex, i.uv);
				float4 strokeCol = tex2D(_StrokeTex, i.uv);

				float3 col = blendColour(canvasCol.rgb, strokeCol.rgb, strokeCol.a);
				return float4(col, max(canvasCol.a,strokeCol.a));
			}
			ENDCG
		}
	}
}