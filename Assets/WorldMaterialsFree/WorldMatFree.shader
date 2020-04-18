// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WorldMatFree"
{
	Properties
	{
		_ColorTint("Color Tint", Color) = (1,1,1,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range( 0.1 , 3)) = 1
		_Metallic("Metallic", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", 2D) = "white" {}
		_SmoothnessStrength("Smoothness Strength", Range( 1 , 2)) = 1
		_Height("Height", 2D) = "white" {}
		_HeightDisplacement("Height Displacement", Range( 0 , 1)) = 0
		_AO("AO", 2D) = "white" {}
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 15
		_Tile("Tile", Range( 1 , 10)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Height;
		uniform float _Tile;
		uniform float _HeightDisplacement;
		uniform float _NormalScale;
		uniform sampler2D _Normal;
		uniform sampler2D _Albedo;
		uniform float4 _ColorTint;
		uniform sampler2D _Metallic;
		uniform float _MetallicStrength;
		uniform sampler2D _Smoothness;
		uniform float _SmoothnessStrength;
		uniform sampler2D _AO;
		uniform float _EdgeLength;

		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 temp_cast_0 = (_Tile).xx;
			float2 uv_TexCoord24 = v.texcoord.xy * temp_cast_0;
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( tex2Dlod( _Height, float4( uv_TexCoord24, 0, 1.0) ) * float4( ase_vertexNormal , 0.0 ) ) * _HeightDisplacement ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_Tile).xx;
			float2 uv_TexCoord24 = i.uv_texcoord * temp_cast_0;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_TexCoord24 ), _NormalScale );
			o.Albedo = ( tex2D( _Albedo, uv_TexCoord24 ) * _ColorTint ).rgb;
			o.Metallic = ( tex2D( _Metallic, uv_TexCoord24 ) * _MetallicStrength ).r;
			o.Smoothness = ( tex2D( _Smoothness, uv_TexCoord24 ) * _SmoothnessStrength ).r;
			o.Occlusion = tex2D( _AO, uv_TexCoord24 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16400
212;100;1364;586;1576.402;327.7562;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;27;-2035.208,107.9987;Float;False;Property;_Tile;Tile;16;0;Create;True;0;0;False;0;1;1;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;24;-1725.752,88.99886;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;9;-392.7047,758.162;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-802.4119,623.3348;Float;True;Property;_Height;Height;8;0;Create;True;0;0;False;0;None;9789d23040cb1fb45ad60392430c3c15;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-840.7454,178.413;Float;True;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;-861.7807,-62.19437;Float;True;Property;_Metallic;Metallic;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;14;-1166.141,-84.35272;Float;False;Property;_NormalScale;Normal Scale;3;0;Create;True;0;0;False;0;1;0;0.1;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-544.2385,66.45996;Float;False;Property;_MetallicStrength;Metallic Strength;5;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-863.4187,-455.3871;Float;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-159.7045,838.162;Float;False;Property;_HeightDisplacement;Height Displacement;9;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-211.9656,-523.9452;Float;False;Property;_ColorTint;Color Tint;0;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-63.70487,645.1623;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-482.1946,284.1693;Float;False;Property;_SmoothnessStrength;Smoothness Strength;7;0;Create;True;0;0;False;0;1;0;1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;20.09167,-255.3173;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-137.1535,20.9689;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-804.5139,405.4511;Float;True;Property;_AO;AO;10;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;141.2956,665.162;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-83.37613,150.4973;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;7;-863.3029,-261.2126;Float;True;Property;_Normal;Normal;2;0;Create;True;0;0;False;0;None;0d2bb5eb5a9fe3541a23173c66da62f2;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;566.278,-34.10064;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;WorldMatFree;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;11;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;24;0;27;0
WireConnection;10;1;24;0
WireConnection;4;1;24;0
WireConnection;17;1;24;0
WireConnection;2;1;24;0
WireConnection;12;0;10;0
WireConnection;12;1;9;0
WireConnection;16;0;2;0
WireConnection;16;1;15;0
WireConnection;18;0;17;0
WireConnection;18;1;19;0
WireConnection;5;1;24;0
WireConnection;13;0;12;0
WireConnection;13;1;11;0
WireConnection;29;0;4;0
WireConnection;29;1;28;0
WireConnection;7;1;24;0
WireConnection;7;5;14;0
WireConnection;0;0;16;0
WireConnection;0;1;7;0
WireConnection;0;3;18;0
WireConnection;0;4;29;0
WireConnection;0;5;5;0
WireConnection;0;11;13;0
ASEEND*/
//CHKSM=9EF584024FD046260157B57A59DA148C2B7B99EF