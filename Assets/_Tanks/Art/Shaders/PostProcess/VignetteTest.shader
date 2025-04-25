Shader "Custom/FullscreenVignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VignetteTex ("Vignette Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 1.0
        [Toggle] _InvertVignette ("Invert Vignette", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "FullscreenVignette"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local _ STEREO_INSTANCING_ON
            #pragma multi_compile_local _ UNITY_SINGLE_PASS_STEREO
            #pragma multi_compile_local _ STEREO_MULTIVIEW_ON
            #pragma multi_compile_local _ STEREO_CUBEMAP_RENDER_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_VignetteTex);
            SAMPLER(sampler_VignetteTex);
            float _Intensity;
            float _InvertVignette;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 vignetteColor = SAMPLE_TEXTURE2D(_VignetteTex, sampler_VignetteTex, input.uv);
                
                // Get vignette value and invert it if needed
                float vignetteValue = vignetteColor.r;
                if (_InvertVignette > 0.5)
                {
                    vignetteValue = 1.0 - vignetteValue;
                }
                
                float3 result = mainColor.rgb * lerp(1.0, vignetteValue, _Intensity);
                
                return float4(result, mainColor.a);
            }
            ENDHLSL
        }
    }
}