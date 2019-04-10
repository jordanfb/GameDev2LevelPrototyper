﻿Shader "Hidden/LozowichEffectShader" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _bwBlend ("Black & White blend", Range (0, 1)) = 0
        _tile ("tileAmount", Range (0, 10)) = 1
        _TonalArtMap("Tonal art map", 2DArray) = "white" {}
        _blurTex1("blurTexture", 2D) = "white" {}
        _blurTex2("blurTexture", 2D) = "white" {}
        _blurTex3("blurTexture", 2D) = "white" {}
        _mitigationAmount("mitigation", Range (0, 100)) = 50
        
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _CameraDepthNormalsTexture;
            uniform sampler2D _blurTex1;
            uniform sampler2D _blurTex2;
            uniform sampler2D _blurTex3;
            uniform float _bwBlend;
            uniform float _tile;
            uniform float _mitigationAmount;
            UNITY_DECLARE_TEX2DARRAY(_TonalArtMap);
            
            float2 rotateUV(float2 inUV, float rotAmount){
                float _RotationSpeed = rotAmount;
                inUV -=0.5;
                float s = sin ( _RotationSpeed);
                float c = cos ( _RotationSpeed);
                float2x2 rotationMatrix = float2x2( c, -s, s, c);
                rotationMatrix *=0.5;
                rotationMatrix +=0.5;
                rotationMatrix = rotationMatrix * 2-1;
                float2 retUV = mul ( inUV, rotationMatrix );
                retUV += 0.5;
                return retUV;
            }
            float rand(float3 myVector)  {
                return frac(sin( dot(myVector ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
            }
            
             float3 AdjustContrast(float3 color, float contrast) {
                return saturate(lerp(half3(0.5, 0.5, 0.5), color, contrast));
            }
            
            float4 frag(v2f_img i) : COLOR {
            
                
                float2 rotUV = rotateUV(i.uv, 1.0);
                float4 blurTex1 = tex2D(_blurTex1, i.uv);
                float4 blurTex2 = tex2D(_blurTex2, i.uv);
                float4 blurTex3 = tex2D(_blurTex3, i.uv);
                
                float2 mainUV = i.uv;
                float4 color;
                float depthValue;
                float3 normalValues;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), depthValue, normalValues);
                //_mitigationAmount* clamp((depthValue * _ProjectionParams.z)/5, 0, 1.0)
                float frame = fmod(_Time.y / 0.2, 3.0);
                int current = floor(frame);
                
                if(current == 0){
                    mainUV.x += (blurTex1.x/_mitigationAmount)%1.0;
                }
                else if(current == 1){
                    mainUV.x += (blurTex2.x/_mitigationAmount)%1.0;
                }
                else if(current == 2){
                    mainUV.x += (blurTex3.x/_mitigationAmount)%1.0;
                }
                
              
            
            
                
                color = tex2D(_MainTex, mainUV);
                
                
                
               
                
                float lum = color.r*.3 + color.g*.59 + color.b*.11;
                lum*=1.8;
                float3 bw = float3( lum, lum, lum ); 
                
                float4 result = color;
                
                fixed texI = (1 - lum) * 8.0;
                float offset = int(_Time.y);
                
                float2 TAMuv = i.uv;
                
               
                
                //if(normalValues.r>0.5){
                    
                //}
                //else if(normalValues.b>0.5){
                //    TAMuv = rotateUV(mainUV, 10.0); 
                //}
                //else{
                //    TAMuv = rotateUV(mainUV, 0);
                //}
                
                
                
               
                
                
               
                TAMuv = rotateUV(mainUV, rand(normalValues)*10); 
                
                if(current == 0){
                    TAMuv.x += 0.01;
                }
                else if(current == 1){
                    TAMuv.x += 0.00;
                }
                else{
                    TAMuv.x += 0.01;
                }
                
                
                
                
                
                
                
                
                float4 col1 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(TAMuv * _tile, floor(texI)));
                float4 col2 = UNITY_SAMPLE_TEX2DARRAY(_TonalArtMap, float3(TAMuv * _tile, ceil(texI)));
                float4 TAMcolor = lerp(col1, col2, texI - floor(texI));
                result.rgb = lerp(color.rgb, TAMcolor, _bwBlend);
                float4 d = tex2D(_CameraDepthTexture, i.uv);
                

                float4 n = float4(normalValues, 1.0);
                //return depthValue * _ProjectionParams.z;
                return result;
                if(depthValue * _ProjectionParams.z){
                    return result;
                }
                else{
                    return color;
                }
                
            }
            ENDCG
        }
    }
}