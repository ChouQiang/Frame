#warning Upgrade NOTE : unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

/*
Created by jiadong chen
http://www.chenjd.me
*/
Shader "Custom/CharactorShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_AnimMap("AnimMap", 2D) = "white" {}
		_AnimStart("_AnimStart", Float) = 0
		_AnimEnd("_AnimEnd", Float) = 0
		_AnimAll("_AnimAll", Float) = 0
		_AnimOff("_AnimOff", Float) = 0
		_Speed("_Speed", Float) = 1
		_Frezz("_Frezz", Float) = 0
		_Alpha("_Alpha", Range(0, 1)) = 1
	}
		SubShader
		{
			Tags{ "RenderType" = "Opaque" "IgnoreProjector" = "True" }
			LOD 100
			Cull off

		Pass
		{
			Tags{ "LIGHTMODE" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members worldNormal)
			#pragma vertex vert
			#pragma fragment frag
					//开启gpu instancing
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata
			{
				float2 uv : TEXCOORD0;
				float4 uv2 : TEXCOORD1;
				float3 normal : NORMAL;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};



			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _AnimAll;
			sampler2D _AnimMap;
			float4 _AnimMap_TexelSize;//x == 1/width

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float, _AnimStart)
				UNITY_DEFINE_INSTANCED_PROP(float, _AnimEnd)
				UNITY_DEFINE_INSTANCED_PROP(float, _AnimOff)
				UNITY_DEFINE_INSTANCED_PROP(float, _Frezz)
				UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
				UNITY_DEFINE_INSTANCED_PROP(float, _Speed)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				float start = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimStart);
				float end = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimEnd);
				float off = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimOff);
	
				float speed = UNITY_ACCESS_INSTANCED_PROP(Props, _Speed);
				float _AnimLen = (end - start);
				float f = (off + _Time.y * speed) / _AnimLen;

				f = fmod(f, 1.0);

				float animMap_x1 = (v.uv2.x * 3 + 0.5) * _AnimMap_TexelSize.x;
				float animMap_x2 = (v.uv2.x * 3 + 1.5) * _AnimMap_TexelSize.x;
				float animMap_x3 = (v.uv2.x * 3 + 2.5) * _AnimMap_TexelSize.x;
				float animMap_y = (f * _AnimLen + start) / _AnimAll;
				float4 row0 = tex2Dlod(_AnimMap, float4(animMap_x1, animMap_y, 0, 0));
				float4 row1 = tex2Dlod(_AnimMap, float4(animMap_x2, animMap_y, 0, 0));
				float4 row2 = tex2Dlod(_AnimMap, float4(animMap_x3, animMap_y, 0, 0));
				float4 row3 = float4(0, 0, 0, 1);
				float4x4 mat = float4x4(row0, row1, row2, row3);
				float4 pos = mul(mat, v.vertex);
				float3 normal = mul(mat, float4(v.normal, 0)).xyz;
				v2f o;
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex = UnityObjectToClipPos(pos);
				o.color = float4(0, 0, 0, 0);
				o.worldNormal = UnityObjectToWorldNormal(normal);


				float3 normalDir = normalize(mul(float4(normal, 0.0), unity_WorldToObject).xyz);

				float frezz = UNITY_ACCESS_INSTANCED_PROP(Props, _Frezz);
				float3 normalWorld = o.worldNormal;
				fixed dotProduct = dot(normalWorld, fixed3(0, 1, 0)) / 2;
				dotProduct = max(0, dotProduct);
				o.color = dotProduct.xxxx * frezz;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				
				float4 col = tex2D(_MainTex, i.uv);
				clip(col.a - 0.2);
				col.rgb += i.color;
				float frezz = UNITY_ACCESS_INSTANCED_PROP(Props, _Frezz);
				float grey = dot(col.rgb, float3(0.299, 0.587, 0.114)) + 0.1;
				col.rgb = frezz * fixed3(grey, grey, grey) + (1 - frezz) * col.rgb;
				col.r -= 0.15 * frezz;
				col.b += 0.15 * frezz;
				float alpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Alpha);
				col.a = alpha;
				return col;
			}
			ENDCG
		}

		}
}
