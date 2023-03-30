Shader "Custom/ToonFaceMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		 _ColorMaskTex ("Color Mask Texture", 2D) = "white" {}
	    _WeaponColorR ("Color Override R value", Color) = (1,1,1,1)
	    _WeaponColorG ("Color Override G value", Color) = (1,1,1,1)
	    _WeaponColorB ("Color Override B value", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		
		Cull off
		Blend SrcAlpha OneMinusSrcAlpha
		
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _ColorMaskTex;
            float4 _ColorMaskTex_ST;
			half4 _ReplaceColorR;
            half4 _ReplaceColorG;
            half4 _ReplaceColorB;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 mask = tex2D(_ColorMaskTex, i.uv);
                float cmask = min(1.0, mask.r + mask.g + mask.b);
				col.rgb = col.rbg * (1 - cmask) + (_ReplaceColorR * mask.r);
				return col;
			}
			ENDCG
		}
	}
}