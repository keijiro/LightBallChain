Shader "Hidden/LightBallChain/Line"
{
    Properties
    {
        [HDR] _Color("", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float4> _Positions;
    float4x4 _Transform;
    half4 _Color;
    float _Radius;

    float4 Vertex(uint vid : SV_VertexID) : POSITION
    {
        uint offs = vid / 2 + (vid & 1);
        float4 p = float4(_Positions[offs].xyz * _Radius, 1);
        return UnityObjectToClipPos(mul(_Transform, p));
    }

    half4 Fragment(float4 position : SV_POSITION) : SV_Target
    {
        return _Color;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
