Shader "Custom/DikkeLijnen" {
    Properties{
        _Color("Color", Color) = (1, 1, 1, 1)
    }
        SubShader{
            Tags {
                "RenderType" = "Opaque"
                "Queue" = "Geometry+3000"
            }
 
            Pass {
                CGPROGRAM
 
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                };
 
                fixed4 _Color;
 
                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    return o;
                }
 
                fixed4 frag(v2f i) : SV_Target
                {
                    return _Color;
                }
                ENDCG
            }
           
            Pass {
                CGPROGRAM
 
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                };
 
                fixed4 _Color;
 
                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.pos.x += o.pos.w / _ScreenParams.x * 0.5;
                    return o;
                }
 
                fixed4 frag(v2f i) : SV_Target
                {
                    return _Color;
                }
                ENDCG
            }
 
            Pass {
                CGPROGRAM
 
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                };
 
                fixed4 _Color;
 
                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.pos.x -= o.pos.w / _ScreenParams.x * 0.5;
                    return o;
                }
 
                fixed4 frag(v2f i) : SV_Target
                {
                    return _Color;
                }
                ENDCG
            }
 
            Pass {
                CGPROGRAM
 
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                };
 
                fixed4 _Color;
 
                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.pos.y += o.pos.w / _ScreenParams.y * 0.5;
                    return o;
                }
 
                fixed4 frag(v2f i) : SV_Target
                {
                    return _Color;
                }
                ENDCG
            }
 
            Pass{
                CGPROGRAM
 
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                };
 
                fixed4 _Color;
 
                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.pos.y -= o.pos.w / _ScreenParams.y * 0.5;
                    return o;
                }
 
                fixed4 frag(v2f i) : SV_Target
                {
                    return _Color;
                }
                ENDCG
            }
 
    }
        FallBack Off
}