Shader "Custom/GoalShader"
{
	Properties
	{
		_MainTex ("Armor Texture", 2D) = "white" {}
		_GradientTex ("Gradient Texture", 2D) = "white" {}
		_GoalColor ("Goal Color", Color) = (1,1,1,1)
		_ArmorColor ("Armor Color", Color) = (1,1,1,1)
		[Toggle(IS_ARMORED)]
        _IsArmored ("Armor on?", float) = 1 
	}
	SubShader
	{
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType"="Plane" }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature ENABLE_IS_ARMORED
			
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
			sampler2D _GradientTex;
			float4 _GradientTex_ST;
			float4 _GoalColor;
			float4 _ArmorColor;
			half _Transition;
			
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
				fixed4 col = tex2D(_GradientTex, i.uv);
				fixed4 _armorTex = tex2D(_MainTex, i.uv);
				#ifdef ENABLE_IS_ARMORED
				col += _armorTex;
				col.rgb *= _ArmorColor;
				#else
				col = _GoalColor;
				#endif
				
				return col;
			}
			ENDCG
		}
	}
}