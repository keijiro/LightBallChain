Shader "Hidden/LightBallChain/Ball"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float4> _Positions;
    float4x4 _Transform;
    float _Radius;
    float _Scale;

    float4 Vertex(float4 position : POSITION, uint id : SV_InstanceID) : POSITION
    {
        position.xyz = position.xyz * _Scale + _Positions[id].xyz * _Radius;
        return UnityObjectToClipPos(mul(_Transform, position));
    }

    half4 Fragment(float4 position : SV_POSITION) : SV_Target
    {
        return 1;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
