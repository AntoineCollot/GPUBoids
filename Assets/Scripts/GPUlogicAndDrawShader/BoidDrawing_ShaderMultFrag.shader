// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/BoidDrawing_ShaderMult"
{
	
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
	Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			  #include "UnityLightingCommon.cginc"
			struct Boid
			{
				float3 position;
				float3 direction;
			};

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0;
                float4 vertex : SV_POSITION;
            };
			
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<Boid> boidsBuffer;
			#endif
			float4x4 _LookAtMatrix;
			float3 _BoidPosition;
			uniform half _BoidSize;
			uniform half _RepeatAreaSize;
			uniform half _BoidCount;
			
			float4x4 look_at_matrix(float3 at, float3 eye, float3 up) {
				float3 zaxis = normalize(at - eye);
				float3 xaxis = normalize(cross(up, zaxis));
				float3 yaxis = cross(zaxis, xaxis);
				return float4x4(
					xaxis.x, yaxis.x, zaxis.x, 0,
					xaxis.y, yaxis.y, zaxis.y, 0,
					xaxis.z, yaxis.z, zaxis.z, 0,
					0, 0, 0, 1
				);
			}
			
			//We get 27 times the number of boids, so we read the modulo of the id and offset it based on the original value
			void setup()
			{
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				uint moduloId = unity_InstanceID%_BoidCount;
				int batch = unity_InstanceID/4000;
				_BoidPosition = boidsBuffer[moduloId].position;
				//Move the boid based on its batch
				_BoidPosition.x += _RepeatAreaSize *((batch / 9) % 3 - 1);
				_BoidPosition.y += _RepeatAreaSize *((batch / 3) % 3 - 1);
				_BoidPosition.z += _RepeatAreaSize *(batch % 3 - 1);
				_LookAtMatrix = look_at_matrix(_BoidPosition, _BoidPosition + (boidsBuffer[moduloId].direction * -1), float3(0.0, 1.0, 0.0));
				#endif
			}

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				o.vertex *= _BoidSize;
				o.vertex = mul(_LookAtMatrix, o.vertex);
				o.vertex.xyz += _BoidPosition;
				#endif
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;

                // the only difference from previous shader:
                // in addition to the diffuse lighting from the main light,
                // add illumination from ambient or light probes
                // ShadeSH9 function from UnityCG.cginc evaluates it,
                // using world space normal
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));
                return o;
            }
			
			sampler2D _MainTex;
        
            fixed4 frag (v2f i) : SV_Target
            {
              /*   // sample the default reflection cubemap, using the reflection vector
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
                // decode cubemap data into actual color
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);
                // output it!
                fixed4 c = 0;
                c.rgb = skyColor; */
				 fixed4 c = tex2D(_MainTex, i.uv);
                c *= i.diff;
                return c;
            }
            ENDCG
		}
	}
}
