﻿Shader "Custom/HolographicShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainColor ("Tint", Color) = (1,1,1,1)
		_FresnelPower ("Fresnel Power", Range(1,5)) = 1
        _FresnelAmount ("Fresnel Amount", Range(0,1)) = 1
        _FresnelColor ("Fresnel Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal_world : TEXCOORD1;
                float3 vertex_world : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            half4 _MainColor;
			float _FresnelPower;
			float _FresnelAmount;
			float4 _FresnelColor;

			void unity_FresnelEffect_float(in float3 normal, in float3 viewDir, in float power, out float Out){
				Out = pow(1 - (saturate(dot(normal, viewDir))), power);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 normal = i.normal_world;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.vertex_world);
            	float fresnel = 0;
            	unity_FresnelEffect_float(normal,viewDir,_FresnelPower,fresnel);
				col.rgb *= _MainColor + fresnel * _FresnelAmount * _FresnelColor;
                return col;
			}
			ENDCG
		}
	}
}