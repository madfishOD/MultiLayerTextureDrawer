Shader "Hidden/MultiLayerTexturePreview"
{
    Properties
    {
		_CheckerTex ("Checker", 2D) = "black" {}
        _MainTex ("Texture", 2D) = "white" {}
		[IntRange] _Mode ("Mode", Range(0, 4)) = 0
    }
    SubShader
    {
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
				float2 uv_2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _CheckerTex;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Mode;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_2 = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 back = tex2D(_CheckerTex, i.uv_2);
                fixed4 tex = tex2D(_MainTex, i.uv);

				fixed4 result;

				if (_Mode == 0) 
				{
					result = fixed4(tex.rgb, 1);
				}
				if (_Mode == 1)
				{
					result = lerp(back, tex.r, tex.r);
				}
				if (_Mode == 2)
				{
					result = lerp(back, tex.g, tex.g);
				}
				if (_Mode == 3)
				{
					result = lerp(back, tex.b, tex.b);
				}
				if (_Mode == 4)
				{
					result = lerp(back, tex.a, tex.a);
				}

                return result;
            }
            ENDCG
        }
    }
}
