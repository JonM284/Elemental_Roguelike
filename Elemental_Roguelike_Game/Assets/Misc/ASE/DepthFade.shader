// Made with Amplify Shader Editor v1.9.1.8
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DepthFade"
{
	Properties
	{
		_ColorClose("ColorClose", Color) = (1,0,0,1)
		_ColorFar("ColorFar", Color) = (1,0,0,1)
		_TransitionDistance("TransitionDistance", Float) = 0
		_TransitionFalloff("TransitionFalloff", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _ColorFar;
		uniform float4 _ColorClose;
		uniform float _TransitionDistance;
		uniform float _TransitionFalloff;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float clampResult100 = clamp( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _TransitionDistance ) , _TransitionFalloff ) , 0.0 , 1.0 );
			float4 lerpResult101 = lerp( _ColorFar , _ColorClose , clampResult100);
			o.Albedo = lerpResult101.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19108
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;89;-2,123;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DepthFade;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;False;Transparent;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.WorldPosInputsNode;93;-1031.726,209.5108;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;94;-1075.494,61.26505;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DistanceOpNode;95;-744.9564,162.8547;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;96;-597.0495,222.3975;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;98;-515.6785,405.4823;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-828.2233,321.8508;Inherit;False;Property;_TransitionDistance;TransitionDistance;3;0;Create;True;0;0;0;False;0;False;0;45.74;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-712.6797,480.7498;Inherit;False;Property;_TransitionFalloff;TransitionFalloff;4;0;Create;True;0;0;0;False;0;False;0;302.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;91;-701.2897,624.0345;Inherit;False;Property;_ColorFar;ColorFar;2;0;Create;True;0;0;0;False;0;False;1,0,0,1;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;92;-701.2898,825.9819;Inherit;False;Property;_ColorClose;ColorClose;1;0;Create;True;0;0;0;False;0;False;1,0,0,1;0.1287875,1,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;101;-264.545,660.4611;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;100;-404.3284,531.0245;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
WireConnection;89;0;101;0
WireConnection;95;0;94;0
WireConnection;95;1;93;0
WireConnection;96;0;95;0
WireConnection;96;1;97;0
WireConnection;98;0;96;0
WireConnection;98;1;99;0
WireConnection;101;0;91;0
WireConnection;101;1;92;0
WireConnection;101;2;100;0
WireConnection;100;0;98;0
ASEEND*/
//CHKSM=1B48765205AF38600B7BE5F24FD684977C429D19