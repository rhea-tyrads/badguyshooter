// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ColorSwap/ColorSwap_Basic"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_AlbedoCutout("Albedo-Cutout", 2D) = "black" {}
		_ColorDetect1Softness("ColorDetect1-Softness", Color) = (1,0,0,0.05882353)
		_NewColor1Gloss("NewColor1-Gloss", Color) = (1,0.8482759,0,0)
		_Range1("Range1", Range( 0 , 1)) = 0.15
		_ColorDetect2Softness("ColorDetect2-Softness", Color) = (0,1,0.213793,0.05882353)
		_NewColor2Gloss("NewColor2-Gloss", Color) = (0,1,0.9172413,0)
		_Range2("Range2", Range( 0 , 1)) = 0.15
		_ColorDetect3Softness("ColorDetect3-Softness", Color) = (0,1,0.213793,0.05882353)
		_NewColor3Metal("NewColor3-Metal", Color) = (0,1,0.9172413,0)
		_Range3("Range3", Range( 0 , 1)) = 0.15
		_OverallEmissive("OverallEmissive", Range( 0 , 1)) = 0.1
		_BaseMetal("BaseMetal", Range( 0 , 1)) = 0
		_BaseGloss("BaseGloss", Range( 0 , 1)) = 0
		_OverallEffect("OverallEffect", Range( 0 , 1)) = 1
		_ProtectionMaskR("ProtectionMask(R)", 2D) = "white" {}
		_MultiplyInBase("MultiplyInBase", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _AlbedoCutout;
		uniform float4 _AlbedoCutout_ST;
		uniform float _MultiplyInBase;
		uniform float4 _NewColor1Gloss;
		uniform sampler2D _ProtectionMaskR;
		uniform float4 _ProtectionMaskR_ST;
		uniform float4 _ColorDetect1Softness;
		uniform float _Range1;
		uniform float4 _NewColor2Gloss;
		uniform float4 _ColorDetect2Softness;
		uniform float _Range2;
		uniform float4 _NewColor3Metal;
		uniform float4 _ColorDetect3Softness;
		uniform float _Range3;
		uniform float _OverallEffect;
		uniform float _OverallEmissive;
		uniform float _BaseMetal;
		uniform float _BaseGloss;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_AlbedoCutout = i.uv_texcoord * _AlbedoCutout_ST.xy + _AlbedoCutout_ST.zw;
			float4 tex2DNode1 = tex2D( _AlbedoCutout, uv_AlbedoCutout );
			float temp_output_2_0_g4 = _MultiplyInBase;
			float temp_output_3_0_g4 = ( 1.0 - temp_output_2_0_g4 );
			float3 appendResult7_g4 = (float3(temp_output_3_0_g4 , temp_output_3_0_g4 , temp_output_3_0_g4));
			float2 uv_ProtectionMaskR = i.uv_texcoord * _ProtectionMaskR_ST.xy + _ProtectionMaskR_ST.zw;
			float4 tex2DNode34 = tex2D( _ProtectionMaskR, uv_ProtectionMaskR );
			float temp_output_35_0 = ( tex2DNode34.r * saturate( ( 1.0 - ( ( distance( _ColorDetect1Softness.rgb , tex2DNode1.rgb ) - _Range1 ) / max( _ColorDetect1Softness.a , 1E-05 ) ) ) ) );
			float4 lerpResult12 = lerp( tex2DNode1 , _NewColor1Gloss , temp_output_35_0);
			float temp_output_36_0 = ( tex2DNode34.r * saturate( ( 1.0 - ( ( distance( _ColorDetect2Softness.rgb , tex2DNode1.rgb ) - _Range2 ) / max( _ColorDetect2Softness.a , 1E-05 ) ) ) ) );
			float4 lerpResult17 = lerp( lerpResult12 , _NewColor2Gloss , temp_output_36_0);
			float temp_output_37_0 = ( tex2DNode34.r * saturate( ( 1.0 - ( ( distance( _ColorDetect3Softness.rgb , tex2DNode1.rgb ) - _Range3 ) / max( _ColorDetect3Softness.a , 1E-05 ) ) ) ) );
			float4 lerpResult23 = lerp( lerpResult17 , _NewColor3Metal , temp_output_37_0);
			float4 lerpResult33 = lerp( tex2DNode1 , lerpResult23 , _OverallEffect);
			float4 temp_output_39_0 = ( float4( ( ( tex2DNode1.rgb * temp_output_2_0_g4 ) + appendResult7_g4 ) , 0.0 ) * lerpResult33 );
			o.Albedo = temp_output_39_0.rgb;
			o.Emission = ( temp_output_39_0 * _OverallEmissive ).rgb;
			float lerpResult31 = lerp( _BaseMetal , _NewColor3Metal.a , temp_output_37_0);
			o.Metallic = lerpResult31;
			float lerpResult24 = lerp( _BaseGloss , _NewColor1Gloss.a , temp_output_35_0);
			float lerpResult25 = lerp( lerpResult24 , _NewColor2Gloss.a , temp_output_36_0);
			float clampResult29 = clamp( lerpResult25 , 0.0 , 1.0 );
			o.Smoothness = clampResult29;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
7;270;1906;773;2380.737;342.3963;2.497339;True;False
Node;AmplifyShaderEditor.ColorNode;3;-803.8552,-17.08619;Float;False;Property;_ColorDetect1Softness;ColorDetect1-Softness;2;0;Create;True;0;0;False;0;1,0,0,0.05882353;0.8980393,0.6313726,0.4,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-892.2068,-245;Float;True;Property;_AlbedoCutout;Albedo-Cutout;1;0;Create;True;0;0;False;0;None;41de176f5cd2a7c4f968ce51a5689095;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-832.6683,337.7882;Float;False;Property;_Range1;Range1;4;0;Create;True;0;0;False;0;0.15;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-816.3908,475.0435;Float;False;Property;_ColorDetect2Softness;ColorDetect2-Softness;5;0;Create;True;0;0;False;0;0,1,0.213793,0.05882353;0.1764706,0.1686275,0.172549,0.05882353;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;16;-879.9841,817.0896;Float;False;Property;_Range2;Range2;7;0;Create;True;0;0;False;0;0.15;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;10;-318.9854,50.14839;Float;False;Color Mask;-1;;1;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;34;-905.6338,-494.048;Float;True;Property;_ProtectionMaskR;ProtectionMask(R);15;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-825.3633,1064.606;Float;False;Property;_ColorDetect3Softness;ColorDetect3-Softness;8;0;Create;True;0;0;False;0;0,1,0.213793,0.05882353;0,1,0.213793,0.05882353;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-888.2982,1408.239;Float;False;Property;_Range3;Range3;10;0;Create;True;0;0;False;0;0.15;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;13;-331.4192,386.2743;Float;False;Color Mask;-1;;2;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-811.4976,158.7279;Float;False;Property;_NewColor1Gloss;NewColor1-Gloss;3;0;Create;True;0;0;False;0;1,0.8482759,0,0;1,0.8482759,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-10.32978,-31.16654;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-820.4832,647.3077;Float;False;Property;_NewColor2Gloss;NewColor2-Gloss;6;0;Create;True;0;0;False;0;0,1,0.9172413,0;0,1,0.9172413,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;18;-299.6289,1174.704;Float;False;Color Mask;-1;;3;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;12;144.2521,-173.3199;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;82.93448,313.4577;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;21;-828.2729,1237.622;Float;False;Property;_NewColor3Metal;NewColor3-Metal;9;0;Create;True;0;0;False;0;0,1,0.9172413,0;0,1,0.9172413,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;149.736,1050.598;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;264.3708,102.1278;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;23;386.5178,452.807;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;38;1016.31,-466.5341;Float;False;Property;_MultiplyInBase;MultiplyInBase;16;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;353.4681,-112.5172;Float;False;Property;_OverallEffect;OverallEffect;14;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-149.1651,-417.0849;Float;False;Property;_BaseGloss;BaseGloss;13;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;40;1390.911,-591.4012;Float;False;Lerp White To;-1;;4;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;33;1132.673,-267.0146;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;24;454.1507,-8.696861;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;515.0352,614.448;Float;False;Property;_OverallEmissive;OverallEmissive;11;0;Create;True;0;0;False;0;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-149.0936,-337.0252;Float;False;Property;_BaseMetal;BaseMetal;12;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;25;733.5909,165.4421;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;1575.714,-399.106;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;1883.404,-86.51048;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;31;660.8321,882.2512;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;29;996.2833,176.5414;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2312.411,220.8553;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;ColorSwap/ColorSwap_Basic;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;1;1;0
WireConnection;10;3;3;0
WireConnection;10;4;11;0
WireConnection;10;5;3;4
WireConnection;13;1;1;0
WireConnection;13;3;14;0
WireConnection;13;4;16;0
WireConnection;13;5;14;4
WireConnection;35;0;34;1
WireConnection;35;1;10;0
WireConnection;18;1;1;0
WireConnection;18;3;20;0
WireConnection;18;4;19;0
WireConnection;18;5;20;4
WireConnection;12;0;1;0
WireConnection;12;1;4;0
WireConnection;12;2;35;0
WireConnection;36;0;34;1
WireConnection;36;1;13;0
WireConnection;37;0;34;1
WireConnection;37;1;18;0
WireConnection;17;0;12;0
WireConnection;17;1;15;0
WireConnection;17;2;36;0
WireConnection;23;0;17;0
WireConnection;23;1;21;0
WireConnection;23;2;37;0
WireConnection;40;1;1;0
WireConnection;40;2;38;0
WireConnection;33;0;1;0
WireConnection;33;1;23;0
WireConnection;33;2;32;0
WireConnection;24;0;28;0
WireConnection;24;1;4;4
WireConnection;24;2;35;0
WireConnection;25;0;24;0
WireConnection;25;1;15;4
WireConnection;25;2;36;0
WireConnection;39;0;40;0
WireConnection;39;1;33;0
WireConnection;27;0;39;0
WireConnection;27;1;26;0
WireConnection;31;0;30;0
WireConnection;31;1;21;4
WireConnection;31;2;37;0
WireConnection;29;0;25;0
WireConnection;0;0;39;0
WireConnection;0;2;27;0
WireConnection;0;3;31;0
WireConnection;0;4;29;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=1974C6D6B2318A672DBE19AD5FA0FD30A40BCC54