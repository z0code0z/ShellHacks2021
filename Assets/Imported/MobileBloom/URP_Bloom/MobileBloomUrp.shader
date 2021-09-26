Shader "RufatShaderlab/BloomURP"
{
	Properties
	{
		[HideInInspector] _MainTex("Base (RGB)", 2D) = "white" {}
	}
	HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	TEXTURE2D_X(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D_X(_BlurTex);
	SAMPLER(sampler_BlurTex);

	half4 _MainTex_TexelSize;
	half4 _MainTex_ST;
	half4 _BloomTex_ST;
	half _BlurAmount;
	half4 _BloomColor;
	half4 _BloomData;


	static const half4 curve[2] = {
		half4(0.5,0.5,0.5,0),
		half4(0.0625, 0.0625,0.0625,0)
	};

	struct appdata {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f {
		half4 pos : POSITION;
#if UNITY_UV_STARTS_AT_TOP
		half4 uv : TEXCOORD0;
#else 
		half2 uv : TEXCOORD0;
#endif	
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	struct v2fb {
		half4 pos : SV_POSITION;
		half4 uv : TEXCOORD0;
		half2 uv1 : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata i)
	{
		v2f o = (v2f)0;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, half4(i.pos.xyz, 1.0h)));
		o.uv.xy = UnityStereoTransformScreenSpaceTex(i.uv);
#if UNITY_UV_STARTS_AT_TOP
		o.uv.zw = o.uv.xy;
		UNITY_BRANCH
		if (_MainTex_TexelSize.y < 0.0)
			o.uv.w = 1.0 - o.uv.w;
#endif
		return o;
	}

	v2fb vertBlur(appdata i)
	{
		v2fb o = (v2fb)0;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, half4(i.pos.xyz, 1.0h)));
		o.uv1 = UnityStereoTransformScreenSpaceTex(i.uv);
		half2 offset = _MainTex_TexelSize * _BlurAmount * (1.0h / _MainTex_ST.xy);
		o.uv = half4(UnityStereoTransformScreenSpaceTex(i.uv - offset), UnityStereoTransformScreenSpaceTex(i.uv + offset));
		return o;
	}

	half4 fragBloom(v2fb i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv1);
		half br = max(c.r, max(c.g, c.b));
		half soft = clamp(br - _BloomData.y, 0.0h, _BloomData.z);
		half a = max(soft * soft * _BloomData.w, br - _BloomData.x) / max(br, 0.00001h);
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.xy);
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.xw);
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.zy);
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.zw);
		return c * a * 0.2h;
	}

	half4 fragBlur(v2fb i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv1) * curve[0];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.xy)* curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.xw) * curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.zy) * curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.zw) * curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, half2(i.uv1.x, i.uv.y)) * curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, half2(i.uv.x, i.uv1.y)) * curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, half2(i.uv1.x, i.uv.w)) * curve[1];
		c += SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, half2(i.uv.z, i.uv1.y)) * curve[1];
		return c;
	}

	half4 frag(v2f i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
#if UNITY_UV_STARTS_AT_TOP
		half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.zw);
#else
		half4 c = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv);
#endif
		c += SAMPLE_TEXTURE2D_X(_BlurTex, sampler_BlurTex, i.uv.xy) * _BloomColor;
		return c;
	}
	ENDHLSL

	Subshader
	{
		Pass //0
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  HLSLPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBloom
		  ENDHLSL
		}

		Pass //1
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  HLSLPROGRAM
		  #pragma vertex vertBlur
		  #pragma fragment fragBlur
		  ENDHLSL
		}

		Pass //2
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  HLSLPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  ENDHLSL
		}
	}
	Fallback off
}