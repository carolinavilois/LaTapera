Shader "LaTapera/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineSize ("Outline Size (UV)", Range(0, 0.05)) = 0.008
        _OutlineOn ("Outline On", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineSize;
                float _OutlineOn;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            float SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 px = float2(_OutlineSize, _OutlineSize);
                float2 uv = IN.uv;

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float centerA = tex.a;

                float n = 0;
                n = max(n, SampleAlpha(uv + float2(px.x, 0)));
                n = max(n, SampleAlpha(uv - float2(px.x, 0)));
                n = max(n, SampleAlpha(uv + float2(0, px.y)));
                n = max(n, SampleAlpha(uv - float2(0, px.y)));
                n = max(n, SampleAlpha(uv + px));
                n = max(n, SampleAlpha(uv - px));
                n = max(n, SampleAlpha(uv + float2(px.x, -px.y)));
                n = max(n, SampleAlpha(uv + float2(-px.x, px.y)));

                float isEdge = (1.0 - step(0.5, centerA)) * step(0.5, n);
                float o = isEdge * clamp(_OutlineOn, 0.0, 1.0);

                half4 col = tex * IN.color * _Color;
                half3 rgb = lerp(col.rgb, _OutlineColor.rgb, o);
                float alpha = max(col.a, o * _OutlineColor.a);
                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
