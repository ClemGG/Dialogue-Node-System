Shader "Custom/Effects/UI/BlendTexture"
{

    Properties
    {
      _MainTex("Background One", 2D) = "white" {}
      _SecTex("Background Two", 2D) = "white" {}
      _Blend("Blend Amount", Range(0.0,1.0)) = 0.0
    }

    SubShader
    {
        Tags
        {
          "Queue"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };



            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SecTex;
            float4 _SecTex_ST;
            float _Blend;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _SecTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = lerp(tex2D(_MainTex, i.uv),
                                  tex2D(_SecTex, i.uv2),
                                  _Blend); 

                return col;
            }
            ENDCG
        }
    }
}
