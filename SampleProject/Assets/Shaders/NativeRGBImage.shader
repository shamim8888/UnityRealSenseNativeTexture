Shader "Custom/NativeRGBImage" {
	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex verts
			#pragma fragment frags
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			
			sampler2D mainTex;
			
			struct VertexPos 
			{
				float4 pos : SV_POSITION;
				float4 wpos : TEXCOORD0;
			};
			
			VertexPos verts(float4 vertex: POSITION)
			{
				VertexPos outVer;
				outVer.pos = mul(UNITY_MATRIX_MVP, vertex);
				outVer.wpos = outVer.pos;
				return outVer;
			}
			
			half4 frags(VertexPos inpVer) : COLOR
			{
				float2 uv;
				uv.x =  0.5 + (inpVer.wpos.x / (inpVer.wpos.w * 2));
				uv.y = 0.5 + (inpVer.wpos.y / (inpVer.wpos.w * 2));
				uv.x = uv.x;
				uv.y = 1 - uv.y;
		
				float3 rgbPixel = tex2D(mainTex, uv);
#ifndef SHADER_API_D3D9
				rgbPixel = rgbPixel.bgr;
#endif
				return half4(rgbPixel, 1.0f);				
			}
			ENDCG
		}
	}
}