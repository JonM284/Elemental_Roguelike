Shader "CardCustom/DisplayBoxShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _DarkColor ("Dark", Color) = (1,1,1,1)
	}
	SubShader
    {
        Name "Toon Shading"
        Tags { "RenderType"="Opaque" "Queue"="Geometry+2"}
        LOD 100
        
         Stencil{
			ref 15
			comp less
            pass replace
            zfail keep
		}
        
        CGINCLUDE
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
                float3 vertex_world : TEXCOORD2;
            };

        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _DarkColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal_world = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))).xyz;
                o.vertex_world = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _DarkColor;
                return col;
            }
            ENDCG
        }
    }
}