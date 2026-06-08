Shader "Custom/OrbParticleAdditiveNoFogDistance"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1, 0, 1, 1)
        _Brightness ("Brightness", Range(0, 20)) = 6
        _Alpha ("Alpha", Range(0, 1)) = 1
        _EdgeCutoff ("Edge Cutoff", Range(0, 1)) = 0.04
        _EdgeSoftness ("Edge Softness", Range(0.001, 1)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay+20"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Lighting Off
        ZWrite Off
        ZTest Always
        Cull Off
        Blend SrcAlpha One
        ColorMask RGB

        Pass
        {
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _TintColor;
            float _Brightness;
            float _Alpha;
            float _EdgeCutoff;
            float _EdgeSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Use the visible brightness of the particle texture as a soft mask.
                // This prevents the square particle card from showing.
                float brightnessMask = max(tex.r, max(tex.g, tex.b));
                float softMask = smoothstep(_EdgeCutoff, _EdgeCutoff + _EdgeSoftness, brightnessMask);

                fixed4 col;
                col.rgb = tex.rgb * i.color.rgb * _TintColor.rgb * _Brightness;
                col.a = tex.a * i.color.a * _TintColor.a * _Alpha * softMask;

                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}