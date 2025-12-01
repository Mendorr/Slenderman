// Copyright (c) 2016 Unity Technologies. MIT license - license_unity.txt
// #NVJOB Fast Shaders Set. MIT license - license_nvjob.txt
// #NVJOB Nicholas Veselov - https://nvjob.github.io


Shader "#NVJOB/Shaders/Fake Light/Flash (Local Position)" {


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


Properties {
//----------------------------------------------

_MainTex("Base (RGB)", 2D) = "white" {}
[HDR] _Color("Main Color", Color) = (1,1,1,1)
_Speed("Blink rate", float) = 1.0
_VectorValue("Brightness Vector Value", Vector) = (0,0,0,0.2)

//----------------------------------------------
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


SubShader {
///////////////////////////////////////////////////////////////////////////////////////////////////////////////

Tags { "RenderType" = "Opaque" "Queue" = "Geometry+700" "DisableBatching" = "True" }
LOD 200
Cull Off

///////////////////////////////////////////////////////////////////////////////////////////////////////////////

Pass {
//----------------------------------------------

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#pragma target 2.0
#include "UnityCG.cginc"

//----------------------------------------------

struct appdata_t {
float4 vertex : POSITION;
float2 texcoord : TEXCOORD0;
UNITY_VERTEX_INPUT_INSTANCE_ID
};

//----------------------------------------------

struct v2f {
float4 vertex : SV_POSITION;
float3 localPos : TEXCOORD0;
float2 uv : TEXCOORD2;
UNITY_VERTEX_OUTPUT_STEREO
};

//----------------------------------------------

sampler2D _MainTex;
float4 _MainTex_ST;
fixed4 _Color;
half _Speed;
half4 _VectorValue;

//----------------------------------------------

v2f vert(appdata_t v) {
v2f o;
UNITY_SETUP_INSTANCE_ID(v);
UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
o.localPos = v.vertex.xyz;
o.vertex = UnityObjectToClipPos(v.vertex);
o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
return o;
}

//----------------------------------------------

fixed4 frag(v2f i) : SV_Target{
half3 wpNorm = normalize(i.localPos);
half vectorSum = (sin(wpNorm.x) * _VectorValue.x) + (sin(wpNorm.y) * _VectorValue.y) + (sin(wpNorm.z) * _VectorValue.z);
half sinTime = sin(vectorSum + (_Time.y * _Speed)) * _VectorValue.w;
fixed4 col = (_Color + sinTime + (_VectorValue.w * 0.5)) * tex2D(_MainTex, i.uv);
UNITY_OPAQUE_ALPHA(col.a);
return col;
}

//----------------------------------------------

ENDCG
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}