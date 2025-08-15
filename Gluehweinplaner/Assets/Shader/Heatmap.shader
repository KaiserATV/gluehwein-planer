Shader "Hidden/Heatmap" {
		Properties{
			_HeatTex("Texture", 2D) = "white" {}
		}
		SubShader{
		Tags{ "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha // Alpha blend

			Pass{
			CGPROGRAM
#pragma vertex vert             
#pragma fragment frag

		struct vertInput {
			float4 pos : POSITION;
		};

		struct vertOutput {
			float4 pos : POSITION;
			fixed3 worldPos : TEXCOORD1;
		};

		vertOutput vert(vertInput input) {
			vertOutput output;
			output.pos = UnityObjectToClipPos(input.pos);
			output.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
			return output;
		}

		uniform float _XDistance;
		uniform float _ZDistance;
		// uniform float _XRandDistance;
		// uniform float _ZRandDistance;
		uniform float _Properties[2000];	// y = intensity
		uniform float2 _MaxVals;
		uniform int _Rows;

		sampler2D _HeatTex;

		half4 frag(vertOutput output) : COLOR{
			int cellX = (_MaxVals.x - output.worldPos.x)/_XDistance;
			int cellZ = (_MaxVals.y - output.worldPos.z)/_ZDistance;
			
			half h = _Properties[_Rows * cellX + cellZ];
			
			half4 color = tex2D(_HeatTex, fixed2(h, 0.5));
			return color;
		}
		ENDCG
	}
}
Fallback "Diffuse"
}
