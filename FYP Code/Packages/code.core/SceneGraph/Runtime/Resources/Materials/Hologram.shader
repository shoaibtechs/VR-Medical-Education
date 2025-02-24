Shader "ORamaVR/XR_HologramShader"
{
	Properties
	{
		// General
		_Brightness("Brightness", Range(0.1, 6.0)) = 3.0
		_Alpha ("Alpha", Range (0.0, 1.0)) = 1.0
		_Direction ("Direction", Vector) = (0,1,0,0)
		// Main Color
		_MainTex ("MainTexture", 2D) = "white" {}
		_MainColor ("MainColor", Color) = (1,1,1,1)
		// Rim/Fresnel
		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Range(0.1, 10)) = 5.0
		_RimSpeed("Rim Speed", Range(0.0, 10)) = 1.0

	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		ColorMask RGB
        Cull Back

		Pass
		{
			CGPROGRAM
			#pragma shader_feature _SCAN_ON
			#pragma shader_feature _GLOW_ON
			#pragma shader_feature _GLITCH_ON
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 worldVertex : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 worldNormal : NORMAL;
				UNITY_VERTEX_OUTPUT_STEREO //Insert
			};

			sampler2D _MainTex;
			float4 _Direction;
			float4 _MainTex_ST;
			float4 _MainColor;
			float4 _RimColor;
			float _RimPower;
			float _RimSpeed;

			float _Brightness;
			float _Alpha;		
			
			v2f vert (appdata v)
			{
				v2f o;
				
				UNITY_SETUP_INSTANCE_ID(v); //Insert
				UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

				o.vertex = UnityObjectToClipPos(v.vertex);
				
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldVertex.xyz));

				return o;
			}

			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texColor = tex2D(_MainTex, i.uv);

				half dirVertex = (dot(i.worldVertex, normalize(float4(_Direction.xyz, 1.0))) + 1) / 2;

				
				// Rim Light
				half rim = 1.0-saturate(dot(i.viewDir, i.worldNormal));
				fixed4 rimColor = _RimColor * pow (rim, _RimPower* ((0.5* sin(_Time.y* _RimSpeed)+0.5)));


				fixed4 col = texColor * _MainColor + rimColor;
				col.a = texColor.a * _Alpha * rim ;

				col.rgb *= _Brightness;

				return col;
			}
			ENDCG
		}
	}
}
