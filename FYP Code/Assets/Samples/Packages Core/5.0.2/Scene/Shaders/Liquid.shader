Shader "Unlit/MAGES/FX/Liquid"
{
    Properties
    {
        [Header(Main)]
        [Toggle] _InvertLiquid("Invert Liquid", Float) = 0
        [HDR] _Tint("Tint", Color) = (1, 1, 1, 1)
        [HDR] _TopColor("Top Color", Color) = (1, 1, 1, 1)
        [Header(Foam)]
        [HDR] _FoamColor("Foam Line Color", Color) = (1, 1, 1, 1)
        _Line("Foam Line Width", Range(0, 0.1)) = 0.0    
        _LineSmooth("Foam Line Smoothness", Range(0, 0.1)) = 0.0    
    }

    SubShader
    {
        Tags { "Queue" = "Transparent-1" "DisableBatching" = "True" }

        Pass
        {
            Zwrite On
            Cull Off
            AlphaToMask On
            Blend SrcAlpha OneMinusSrcAlpha 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;	

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 viewDir : COLOR;
                float3 normal : COLOR2;		
                float3 fillPosition : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 _FillAmount;
            float _VarianceX, _VarianceY;
            float4 _TopColor, _FoamColor, _Tint;
            float _Line, _LineSmooth;
            float _InvertLiquid;

            // Unity Shadergraph Rotate About Axis Node
            // Source: https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Rotate-About-Axis-Node.html
            float3 Unity_RotateAboutAxis_Degrees_float(float3 In, float3 Axis, float Rotation)
            {
                Rotation = radians(Rotation);
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);
                float3x3 rot_mat = 
                {
                    one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                float3 Out = mul(rot_mat, In);
                return Out;
            }

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex.xyz);
                float3 worldPosOffset = float3(worldPos.x, worldPos.y, worldPos.z) - _FillAmount;
                float3 worldPosX = Unity_RotateAboutAxis_Degrees_float(worldPosOffset, float3(0, 0, 1), 90);
                float3 worldPosZ = Unity_RotateAboutAxis_Degrees_float(worldPosOffset, float3(1, 0, 0), 90);
                float3 worldPosAdjusted = worldPos + (worldPosX * _VarianceX) + (worldPosZ * _VarianceY);

                o.fillPosition = _InvertLiquid ? -worldPosAdjusted - _FillAmount : worldPosAdjusted - _FillAmount;

                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.normal = v.normal;
                o.worldNormal = mul((float4x4)unity_ObjectToWorld, v.normal);

                return o;
            }

            fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 worldNormal = mul(unity_ObjectToWorld, float4(i.normal, 0.0)).xyz;

                // Add Deformation using waves
                float varianceIntensity = abs(_VarianceX) + abs(_VarianceY);
                float variance = sin((i.fillPosition.x * 2) + (i.fillPosition.z * 2) + (_Time.y)) * (0.04f * varianceIntensity);
                float movingfillPosition = i.fillPosition.y + variance;

                fixed4 col = _Tint;
                UNITY_APPLY_FOG(i.fogCoord, col);

                float cutoffTop = step(movingfillPosition, 0.5);
                float foam = cutoffTop * smoothstep(0.5 - _Line - _LineSmooth, 0.5 - _Line, movingfillPosition);
                float4 foamColored = foam * _FoamColor;

                float result = cutoffTop - foam;
                float4 resultColored = result * col;

                float4 finalResult = resultColored + foamColored;

                float backfaceFoam = (cutoffTop * smoothstep(0.5 - (0.2 * _Line) - _LineSmooth, 0.5 - (0.2 * _Line), movingfillPosition));
                float4 backfaceFoamColor = _FoamColor * backfaceFoam;

                float4 topColor = (_TopColor * (1 - backfaceFoam) + backfaceFoamColor) * (foam + result);

                clip(result + foam - 0.01);

                return facing > 0 ? finalResult : topColor;
            }
            ENDCG
        }
    }
}