Shader "Custom/TiltIndicationByRotation"
{
    Properties
    {
        _MainTex ("Wood Texture", 2D) = "white" {}                  // Wood texture
        _TopColor ("Top Color (Yellow)", Color) = (1, 1, 0, 1)      // Color for the upward side
        _BottomColor ("Bottom Color (Red)", Color) = (1, 0, 0, 1)   // Color for the downward side
        _PositionOffset("Position Offset", float) = 0               // Initial position offset
        _EdgeFalloff("Edge Falloff", float) = .5                    // Controls color falloff towards center 
        _EdgeDist("Edge Dist", float) = 3                           // Controls the distance from center to start the effect
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

            sampler2D _MainTex;
            float4 _TopColor;
            float4 _BottomColor;
            float _PositionOffset;
            float _EdgeFalloff;
            float _EdgeDist;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  // Gets world position of the vertex
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, i.uv);

                // Calculate the tilt of the platform based on world Y position. Higher Y positions will represent the "up" side, lower ones the "down" side
                float tiltFactor = i.worldPos.y + _PositionOffset;

                // Remap tiltFactor to range [0, 1] for blending colors
                tiltFactor = saturate(tiltFactor);

                // Blend between top color (yellow) and bottom color (red)
                float4 slopeColor = lerp(_BottomColor, _TopColor, tiltFactor);

                // Calculate distance from the center to fade the effect toward the edges
                float distToCenter = length(i.worldPos.xz);  // Assume xz plane is the platform surface
                float edgeIntensity = smoothstep(1.0 - _EdgeFalloff, _EdgeDist, distToCenter);
                
                float4 finalColor = lerp(texColor, slopeColor, edgeIntensity);
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
