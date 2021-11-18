Shader "Hidden/AddShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
        ZWrite Off
        ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionHCS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS : SV_POSITION;
                float2  uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

			Varyings vert(Attributes input)
			{
				Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = float4(input.positionHCS.xyz, 1.0);

                #if UNITY_UV_STARTS_AT_TOP
                output.positionCS.y *= -1;
                #endif

				output.uv = input.uv;
				return output;
			}
			
			TEXTURE2D_X(_MainTex);
			SAMPLER(sampler_MainTex);
			float _Sample;

			float4 frag(Attributes input) : SV_Target
			{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				return float4(SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv).rgb, 1.0f / (_Sample + 1.0f));
			}
			ENDHLSL
		}
	}
}