Shader "#NVJOB/Shaders/Specular/Alpha cutoff" {


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	

Properties {
//----------------------------------------------

_Color ("Main Color", Color) = (1,1,1,1)
_Emission("Emission Color", Color) = (0, 0, 0, 1)
_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
_BumpMap ("Normalmap", 2D) = "bump" {}
_IntensityNm("Intensity Normalmap", Range(-20, 20)) = 1
[Enum(Back,2,Off,0)] _Cull("Backface Culling", Int) = 2
_Cutoff("Alpha cutoff", Range(0,1)) = 0.5

//----------------------------------------------
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


SubShader{
///////////////////////////////////////////////////////////////////////////////////////////////////////////////

Tags {"Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque"}
Cull[_Cull]
LOD 200

CGPROGRAM
#pragma surface surf BlinnPhong exclude_path:prepass nolightmap nometa noforwardadd nolppv noshadowmask noforwardadd interpolateview novertexlights

//----------------------------------------------

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color, _Emission;
half _Shininess, _IntensityNm, _Cutoff;

//----------------------------------------------

struct Input {
float2 uv_MainTex;
float2 uv_BumpMap;
};

//----------------------------------------------

void surf (Input IN, inout SurfaceOutput o) {
fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
o.Albedo = tex.rgb * _Color.rgb;
clip(tex.a - _Cutoff);
o.Gloss = tex.a;
o.Specular = _Shininess;

fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
normal.x *= _IntensityNm;
normal.y *= _IntensityNm;
o.Normal = normalize(normal);

o.Emission = _Emission.rgb;
}

//----------------------------------------------

ENDCG

///////////////////////////////////////////////////////////////////////////////////////////////////////////////
}


Fallback "Legacy Shaders/VertexLit"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}