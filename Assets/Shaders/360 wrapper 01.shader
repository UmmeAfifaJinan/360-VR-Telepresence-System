// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "Custom/360Wrapper01" {
	Properties{
		_Tint("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
		_Rotation("Rotation", Range(0, 360)) = 0

		//用于控制非透明方向
		_TransDir("Transparent Direction", Range(-180, 180)) = 0

		//用于控制非透明区域大小
		_TransArea("Transparent Area", Range(10, 180)) = 10

		// 用于在透明纹理的基础上控制整体的透明度
		_AlphaScale("Alpha Scale", Range(0,1)) = 1
		[NoScaleOffset] _Tex("Panorama (HDR)", 2D) = "grey" {}
	}

		SubShader{
			 Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			 ZWrite off Cull Front
			// 这是混合模式
			Blend SrcAlpha OneMinusSrcAlpha
			
			Pass {

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _Tex;
				half4 _Tex_HDR;
				half4 _Tint;
				half _Exposure;
				float _Rotation;
				float _TransArea;
				float _TransDir;
				fixed _AlphaScale;

				float4 RotateAroundYInDegrees(float4 vertex, float degrees)
				{
					float alpha = degrees * UNITY_PI / 180.0;
					float sina, cosa;
					sincos(alpha, sina, cosa);
					float2x2 m = float2x2(cosa, -sina, sina, cosa);
					return float4(mul(m, vertex.xz), vertex.yw).xzyw;
				}

				struct appdata_t {
					float4 vertex : POSITION;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float3 texcoord : TEXCOORD0;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(RotateAroundYInDegrees(v.vertex, _Rotation));
					o.texcoord = v.vertex.xyz;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float3 dir = normalize(i.texcoord);
					float2 longlat = float2(atan2(dir.x, dir.z) + UNITY_PI, acos(-dir.y));
					float2 uv = longlat / float2(2.0 * UNITY_PI, UNITY_PI);
					half4 tex = tex2D(_Tex, uv);
					half3 c = DecodeHDR(tex, _Tex_HDR);
					//float trans1 = -(atan2(dir.x, dir.z) * 180 / UNITY_PI - _TransDir)*(atan2(dir.x, dir.z) * 180 / UNITY_PI - _TransDir) / (_TransArea*_TransArea) + 1;
					//float trans2 = -(atan2(dir.x, dir.z) * 180 / UNITY_PI - _TransDir - 360)*(atan2(dir.x, dir.z) * 180 / UNITY_PI - _TransDir - 360) / (_TransArea*_TransArea) + 1;
					//float trans3 = -(atan2(dir.x, dir.z) * 180 / UNITY_PI - _TransDir + 360)*(atan2(dir.x, dir.z) * 180 / UNITY_PI - _TransDir + 360) / (_TransArea*_TransArea) + 1;

					//return half4(c, _AlphaScale * trans1 * (1-step(trans1, 0)) + _AlphaScale * trans2 * (1 - step(trans2, 0)) + _AlphaScale * trans3 * (1 - step(trans3, 0)));
					return half4(c, _AlphaScale);
				}
				ENDCG
			}
	}


		Fallback "Diffuse"
}
