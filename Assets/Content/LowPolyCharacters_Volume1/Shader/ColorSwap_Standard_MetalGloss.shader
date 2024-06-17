// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ColorSwap/ColorSwap_StandardMetalGloss"
{
	Properties
	{
		_AlbedoTexture("Albedo Texture", 2D) = "black" {}
		_ColorDetect1Softness("ColorDetect1-Softness", Color) = (1,0,0,0)
		_NewColor1("NewColor1", Color) = (1,0.8482759,0,0)
		_Range1("Range1", Range( 0 , 1)) = 0
		_ColorDetect2Softness("ColorDetect2-Softness", Color) = (0,1,0.213793,0)
		_NewColor2("NewColor2", Color) = (0,1,0.9172413,0)
		_Range2("Range2", Range( 0 , 1)) = 0
		_ColorDetect3Softness("ColorDetect3-Softness", Color) = (0,1,0.213793,0)
		_NewColor3("NewColor3", Color) = (0,1,0.9172413,0)
		_Range3("Range3", Range( 0 , 1)) = 0
		_OverallEmissive("OverallEmissive", Range( 0 , 1)) = 0.1
		_NormalMap("NormalMap", 2D) = "bump" {}
		_MetalGloss("Metal-Gloss", 2D) = "black" {}
		_OverallEffect("OverallEffect", Range( 0 , 1)) = 1
		_OverrideGloss("OverrideGloss", Range( 0 , 1)) = 0
		_ProtectionMaskR("ProtectionMask(R)", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _AlbedoTexture;
		uniform float4 _AlbedoTexture_ST;
		uniform float4 _NewColor1;
		uniform sampler2D _ProtectionMaskR;
		uniform float4 _ProtectionMaskR_ST;
		uniform float4 _ColorDetect1Softness;
		uniform float _Range1;
		uniform float4 _NewColor2;
		uniform float4 _ColorDetect2Softness;
		uniform float _Range2;
		uniform float4 _NewColor3;
		uniform float4 _ColorDetect3Softness;
		uniform float _Range3;
		uniform float _OverallEffect;
		uniform float _OverallEmissive;
		uniform sampler2D _MetalGloss;
		uniform float4 _MetalGloss_ST;
		uniform float _OverrideGloss;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_AlbedoTexture = i.uv_texcoord * _AlbedoTexture_ST.xy + _AlbedoTexture_ST.zw;
			float4 tex2DNode1 = tex2D( _AlbedoTexture, uv_AlbedoTexture );
			float2 uv_ProtectionMaskR = i.uv_texcoord * _ProtectionMaskR_ST.xy + _ProtectionMaskR_ST.zw;
			float4 tex2DNode54 = tex2D( _ProtectionMaskR, uv_ProtectionMaskR );
			float temp_output_55_0 = ( tex2DNode54.r * saturate( ( 1.0 - ( ( distance( _ColorDetect1Softness.rgb , tex2DNode1.rgb ) - _Range1 ) / max( _ColorDetect1Softness.a , 1E-05 ) ) ) ) );
			float4 lerpResult12 = lerp( tex2DNode1 , _NewColor1 , temp_output_55_0);
			float temp_output_56_0 = ( tex2DNode54.r * saturate( ( 1.0 - ( ( distance( _ColorDetect2Softness.rgb , tex2DNode1.rgb ) - _Range2 ) / max( _ColorDetect2Softness.a , 1E-05 ) ) ) ) );
			float4 lerpResult17 = lerp( lerpResult12 , _NewColor2 , temp_output_56_0);
			float temp_output_57_0 = ( tex2DNode54.r * saturate( ( 1.0 - ( ( distance( _ColorDetect3Softness.rgb , tex2DNode1.rgb ) - _Range3 ) / max( _ColorDetect3Softness.a , 1E-05 ) ) ) ) );
			float4 lerpResult23 = lerp( lerpResult17 , _NewColor3 , temp_output_57_0);
			float4 lerpResult48 = lerp( tex2DNode1 , lerpResult23 , _OverallEffect);
			o.Albedo = lerpResult48.rgb;
			o.Emission = ( lerpResult48 * _OverallEmissive ).rgb;
			float2 uv_MetalGloss = i.uv_texcoord * _MetalGloss_ST.xy + _MetalGloss_ST.zw;
			float4 tex2DNode33 = tex2D( _MetalGloss, uv_MetalGloss );
			o.Metallic = tex2DNode33.r;
			float clampResult53 = clamp( ( temp_output_55_0 + temp_output_56_0 + temp_output_57_0 ) , 0.0 , 1.0 );
			float lerpResult49 = lerp( tex2DNode33.a , _OverrideGloss , ( clampResult53 * _OverrideGloss * tex2DNode54.r ));
			o.Smoothness = lerpResult49;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
245;610;1906;773;2350.767;733.4831;3.251539;True;False
Node;AmplifyShaderEditor.ColorNode;3;-849.3215,-13.53614;Float;False;Property;_ColorDetect1Softness;ColorDetect1-Softness;1;0;Create;True;0;0;False;0;1,0,0,0;0.2352941,0.2705882,0.1921569,0.278;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-832.6683,337.7882;Float;False;Property;_Range1;Range1;3;0;Create;True;0;0;False;0;0;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-894.4459,-245;Float;True;Property;_AlbedoTexture;Albedo Texture;0;0;Create;True;0;0;False;0;None;87ae3159010e2b14fb9aaa521344b200;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-825.3633,1064.606;Float;False;Property;_ColorDetect3Softness;ColorDetect3-Softness;7;0;Create;True;0;0;False;0;0,1,0.213793,0;0.6235402,0.8382353,0.3698097,0.216;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;44;-886.8818,1401.04;Float;False;Property;_Range3;Range3;9;0;Create;True;0;0;False;0;0;0.19;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;54;-852.0885,-529.0224;Float;True;Property;_ProtectionMaskR;ProtectionMask(R);15;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;10;-343.5204,110.32;Float;False;Color Mask;-1;;1;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-816.3908,475.0435;Float;False;Property;_ColorDetect2Softness;ColorDetect2-Softness;4;0;Create;True;0;0;False;0;0,1,0.213793,0;0.5165983,0.9558824,0.07731403,0.203;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;16;-882.1542,817.0896;Float;False;Property;_Range2;Range2;6;0;Create;True;0;0;False;0;0;0.419;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;13;-283.8595,485.9234;Float;False;Color Mask;-1;;4;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2.187722,54.32325;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-811.4976,158.7279;Float;False;Property;_NewColor1;NewColor1;2;0;Create;True;0;0;False;0;1,0.8482759,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;18;-267.3507,718.2179;Float;False;Color Mask;-1;;3;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;129.3611,397.8456;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;12;165.9703,-146.0661;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;119.4686,627.1551;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-820.4832,647.3077;Float;False;Property;_NewColor2;NewColor2;5;0;Create;True;0;0;False;0;0,1,0.9172413,0;0.3830203,0.7132353,0.277952,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;21;-828.2729,1235.687;Float;False;Property;_NewColor3;NewColor3;8;0;Create;True;0;0;False;0;0,1,0.9172413,0;1,0.9645031,0.4852941,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;52;898.6749,523.4581;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;336.62,45.73666;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;50;963.4562,1216.37;Float;False;Property;_OverrideGloss;OverrideGloss;14;0;Create;True;0;0;False;0;0;0.559;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;23;612.3005,345.7265;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;46;275.3361,987.6365;Float;False;Property;_OverallEffect;OverallEffect;13;0;Create;True;0;0;False;0;1;0.44;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;53;1195.147,509.244;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;48;967.624,99.70224;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;33;957.7647,993.9528;Float;True;Property;_MetalGloss;Metal-Gloss;12;0;Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;1385.435,1257.107;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;411.5321,584.064;Float;False;Property;_OverallEmissive;OverallEmissive;10;0;Create;True;0;0;False;0;0.1;0.008;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;49;1565.312,1146.733;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;1502.964,288.8987;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;32;1002.876,703.1668;Float;True;Property;_NormalMap;NormalMap;11;0;Create;True;0;0;False;0;None;5ca6cfab780c1144dba9d5415ec814c7;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2150.998,331.9736;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;ColorSwap/ColorSwap_StandardMetalGloss;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;1;1;0
WireConnection;10;3;3;0
WireConnection;10;4;11;0
WireConnection;10;5;3;4
WireConnection;13;1;1;0
WireConnection;13;3;14;0
WireConnection;13;4;16;0
WireConnection;13;5;14;4
WireConnection;55;0;54;1
WireConnection;55;1;10;0
WireConnection;18;1;1;0
WireConnection;18;3;20;0
WireConnection;18;4;44;0
WireConnection;18;5;20;4
WireConnection;56;0;54;1
WireConnection;56;1;13;0
WireConnection;12;0;1;0
WireConnection;12;1;4;0
WireConnection;12;2;55;0
WireConnection;57;0;54;1
WireConnection;57;1;18;0
WireConnection;52;0;55;0
WireConnection;52;1;56;0
WireConnection;52;2;57;0
WireConnection;17;0;12;0
WireConnection;17;1;15;0
WireConnection;17;2;56;0
WireConnection;23;0;17;0
WireConnection;23;1;21;0
WireConnection;23;2;57;0
WireConnection;53;0;52;0
WireConnection;48;0;1;0
WireConnection;48;1;23;0
WireConnection;48;2;46;0
WireConnection;51;0;53;0
WireConnection;51;1;50;0
WireConnection;51;2;54;1
WireConnection;49;0;33;4
WireConnection;49;1;50;0
WireConnection;49;2;51;0
WireConnection;45;0;48;0
WireConnection;45;1;26;0
WireConnection;0;0;48;0
WireConnection;0;1;32;0
WireConnection;0;2;45;0
WireConnection;0;3;33;0
WireConnection;0;4;49;0
ASEEND*/
//CHKSM=4F9476A17C5357FEDDEE24BE0B23A688B0471D82