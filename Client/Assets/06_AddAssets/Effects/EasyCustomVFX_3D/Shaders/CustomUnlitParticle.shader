Shader "Unlit/CustomUnlitParticle"
{
    Properties
    {
        //Main
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("COLOR",color) = (1,1,1,1)
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Pass
        {
            Blend SrcAlpha One
            Cull Back
            Lighting Off 
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color; // 頂点カラーとプロパティカラーを掛け合わせる
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                half4 finalColor = texColor * i.color;
                finalColor *= i.color.a; // 頂点カラーのアルファと乗算
                finalColor.a = texColor.a; // アルファはテクスチャのアルファを使用
                return finalColor;
            }
            ENDHLSL
        }
    }
}
