
// This code is related to an answer I provided in the Unity forums at:
// http://forum.unity3d.com/threads/circular-fade-in-out-shader.344816/

// The code has a small fix to the original for Direct3D like displays
// UNITY_UV_STARTS_AT_TOP is only set for Direct3D, Metal and Consoles
// There is an issue with the image being flipped on these devices
// The changes below can be seen on the lines with '_MainTex_TexelSize'
// Thanks to Peter77 for this fine shader

Shader "Custom/Effects/UI/ScreenTransitionImageEffect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_MaskValue("Mask Value", Range(0,1)) = 0.5
		_MaskSpread("Mask Spread", Range(0,1)) = 0.5
		_MaskColor("Mask Color", Color) = (0,0,0,1)
		[Toggle(INVERT_MASK)] _INVERT_MASK("Mask Invert", Float) = 0
		[KeywordEnum(Mask, Fade)] _FadeMode("Fade mode", Float) = 0
}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Tags
		{
			"Queue" = "Overlay"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag      
			#include "UnityCG.cginc"

			#pragma shader_feature INVERT_MASK

		float4 _MainTex_TexelSize;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
			#endif

				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;
			float _FadeMode;
			float _MaskValue;
			float _MaskSpread;
			float4 _MaskColor;

			fixed4 frag(v2f i) : SV_Target
			{

				float4 col = tex2D(_MainTex, i.uv);
				float4 mask = tex2D(_MaskTex, i.uv);

				// Scale 0..255 to 0..254 range.
				float alpha = mask.a * (1 - 1 / 255.0);

			#if INVERT_MASK
				alpha = 1 - alpha;
			#endif

				// If the mask value is greater than the alpha value,
				// we want to draw the mask.
				// EDIT: Le smoothstep nous permet une transition plus douce

				//float weight = step(_MaskValue, alpha); 
				float weight = smoothstep(_MaskValue, _MaskValue - _MaskSpread, alpha);





				// On fait un lerp entre le fade et le mask si jamais on veut juste un fondu vers le noir au lieu d'une transition animée
				col.rgb = lerp(
					lerp(col.rgb, lerp(_MaskColor.rgb, col.rgb, weight), _MaskColor.a),	//Mask
					lerp(_MaskColor.rgb, col.rgb, saturate(_MaskValue)),				//Fade
					_FadeMode
				);


				return col;
			}
			ENDCG
		}
	}
}