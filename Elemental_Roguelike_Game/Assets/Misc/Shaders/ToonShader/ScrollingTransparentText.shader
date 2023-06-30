Shader "Custom/ScrollingTransparentText"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_ScrollSpeed ("Scroll Speed", float) = 0.5
		_Alpha ("Alpha Value", range(0.0,1.0)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  }
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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _ScrollSpeed;
			float _Alpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
                uv.y += _ScrollSpeed * _Time.y; // _Time.y provides a time-based value for animation

                // Sample the texture and apply transparency
                fixed4 col = tex2D(_MainTex, uv);
                col.a *= _Alpha; // Adjust the alpha value as needed

                return col * _Color;
			}
			ENDCG
		}
	}
}