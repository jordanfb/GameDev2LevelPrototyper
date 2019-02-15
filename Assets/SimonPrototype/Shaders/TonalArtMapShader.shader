﻿Shader "Custom/TonalArtMapShader" {
	Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_TonalArtMap ("Tonal Art Map", 2DArray) = "" {}
		_ColorTint ("Tint", Color) = (1.0, 0.0, 0.0, 1.0)
        _Contrast("contrast", Range(1, 10)) = 1
        _Levels("number of gradient levels", Range(0, 10))= 0
        _RampTex("Ramp", 2D) = ""{}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        
	}

	SubShader {
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

		CGPROGRAM

		#pragma surface surf CelShadingForward finalcolor:apply_tam fullforwardshadows
       
		struct Input {
            float3 worldPos;
			float2 uv_MainTex;
			float2 uv_TonalArtMap;
            float3 worldNormal;
            float2 uv_BumpMap;
            //INTERNAL_DATA
            
		};

		float _Test;
		fixed4 _ColorTint;
		sampler2D _MainTex;
        sampler2D _BumpMap;
        UNITY_DECLARE_TEX2DARRAY(_TonalArtMap);
        float4 _TonalArtMap_ST;
        float _Contrast;
        float _Levels;
        float2 worldUV;
        
        sampler2D _RampTex;
      
        fixed4 LightingToon(SurfaceOutput s, fixed3 lightDir, fixed atten){
            half NdotL = dot(s.Normal, lightDir);
            
            NdotL = tex2D(_RampTex, fixed2(NdotL, 0.5));
            
            half4 color;
            
            color.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
            color.a = s.Alpha;
            
            
            return color;
            
            
        
        }
        half4 LightingCelShadingForward(SurfaceOutput s, half3 lightDir, half atten) {
            half NdotL = dot(s.Normal, lightDir);
            if (NdotL <= 0.0){
                NdotL = 0;
            }
            else if(NdotL<=0.5){
                NdotL = 0.5;
            }
            else{
                NdotL = 1;
            }
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
            c.a = s.Alpha;
            return c;
        }
      

	
		float luma(fixed4 color) {
			const fixed4 luma_vec = float4(0.2126, 0.7152, 0.0722, 1.0);
			return dot(color, luma_vec);
		}
        
        half4 AdjustContrast(half4 color, half contrast) {
            return saturate(lerp(half4(0.5, 0.5, 0.5, 1.0), color, contrast));
        }
        
		void apply_tam(Input IN, SurfaceOutput o, inout fixed4 color)
		{
			//fixed l = pow(luma(color), 2.2);
			fixed l = luma(color);
			fixed texI = (1 - l) * _Levels;
            float2 worldUV = IN.uv_TonalArtMap;
            
            fixed4 col1;
            fixed4 col2;
            
            
            if(abs(IN.worldNormal.y) > 0.5)
            {
                float2 worldUV = TRANSFORM_TEX(IN.worldPos.xz, _TonalArtMap);
                col1 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(worldUV, floor(texI)));
                col2 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(worldUV, ceil(texI)));

            }
            else if(abs(IN.worldNormal.x) > 0.5)
            {
                float2 worldUV = TRANSFORM_TEX(IN.worldPos.yz, _TonalArtMap);
                col1 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(worldUV, floor(texI)));
                col2 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(worldUV, ceil(texI)));

            }
            else
            {
                float2 worldUV = TRANSFORM_TEX(IN.worldPos.xy, _TonalArtMap);
                col1 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(worldUV, floor(texI)));
                col2 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(worldUV, ceil(texI)));

            }
            
            col1 = AdjustContrast(col1, _Contrast) * _ColorTint;
            col2 = AdjustContrast(col2, _Contrast) * _ColorTint;
            
           
            
            color = lerp(col1, col2, texI - floor(texI))*tex2D(_MainTex, IN.uv_MainTex);
            
           
            
            
           
           
           
            
            //fixed4 col1 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(IN.uv_TonalArtMap, floor(texI)));
            //fixed4 col2 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(IN.uv_TonalArtMap, ceil(texI)));

            //color = pow(lerp(col1, col2, texI - floor(texI)),_Mod);
            
			
			
		}

		void surf (Input IN, inout SurfaceOutput o) {
           
           
            o.Albedo =  tex2D(_MainTex, IN.uv_MainTex);
            //o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
            float3 worldNormal = WorldNormalVector (IN, o.Normal);
		    
		}
        

		ENDCG
	}
	Fallback "Diffuse"
}
