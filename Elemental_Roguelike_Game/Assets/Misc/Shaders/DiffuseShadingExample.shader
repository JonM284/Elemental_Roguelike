Shader "Learning_Lighting/DiffuseShadingExample"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainColor ("Color", Color) = (1,1,1,1)
		_OutlineCol ("Outline Color", Color) = (0,0,0,1)
		_OutlineOffset ("Outline Offset", Range(0,1)) = 0
		_ShadingInt ("Shading Intensity", Range(0,.9)) = 0
		_LightInt ("Light Instensity", Range(0,1)) = 1
		_Division ("Division", Range(0,200)) = 100
		[IntRange]
		_Sections ("Sections", Range(1,10)) = 5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100
		
		CGINCLUDE

		#pragma vertex vert
		#pragma fragment frag

		sampler2D _MainTex;
		float4 _MainTex_ST;
		
		ENDCG
		
		/*
			Diffuse Reflection
			-a surface can have two types of reflection:
			1. Matte
			2. Gloss

			Diffuse => obeys Lambert's cosine law

			Equation:
			D= Dr [reflection color of light source] * Dl [intensity] max(0, N * L)

			Diffusion is calculated by the angle between the surface normal [N] and lighting direction [L]
			*Note: this corresponds to the dot product between these two properties.

			[Dr] [Dl] = the amount of reflection in terms of color and intensity
			
			*/

		Pass
		{
			Tags{ "LightMode"="ForwardBase" }
			
			CGPROGRAM
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal_world : TEXCOORD1;
			};

			
			float4 _MainColor;
			half _LightInt;
			half _Sections;
			half _Division;
			half _ShadingInt;
			float4 _LightColor0;

			float3 LambertShading(float3 colorRefl, float lightInt, float3 normal, float3 lightDir)
			{
				return colorRefl * lightInt * max(_ShadingInt, dot(normal, lightDir));
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal_world = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float3 normal = i.normal_world;
				//Direction of Directional environmental lighting
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				//Take only the rgb values of the light color (directional environmental lighting)
				fixed3 colorRefl = _LightColor0.rgb;
				float3 diffuse = LambertShading(colorRefl, _LightInt, normal, lightDir);
				float fl = ceil(diffuse * _Sections) * (_Sections/_Division);
				col.rgb *= fl;
				
				return col * _MainColor;
			}
			ENDCG
		}
		
		Pass
		{
			Tags{"RenderType"="Opaque"}
			Cull Front
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4 _OutlineCol;
			float _OutlineOffset;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex * (_OutlineOffset + 1));
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return  _OutlineCol;
			}
			
			ENDCG
		}
	}
}