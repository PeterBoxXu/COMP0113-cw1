Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        // 这个是原示例就有的纹理属性，保留以防日后需要使用贴图
        _MainTex ("Texture", 2D) = "white" {}

        // 以下四个是挖孔叠加所需的属性
        _Color ("Overlay Color (叠加颜色)", Color) = (0,0,0,1)
        _HoleRadius ("Hole Radius (孔半径)", Range(0,1)) = 0.3
        _HoleCenter ("Hole Center (孔中心)", Vector) = (0.5, 0.5, 0, 0)
        _HoleFeather ("Hole Feather (柔边宽度)", Range(0,0.5)) = 0.05
    }

    SubShader
    {
        // 与原先的UI Shader相同：使用Transparent队列、关闭剔除、关闭ZWrite等
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        LOD 100

        // 源Alpha和1 - 源Alpha进行混合，支持半透明叠加
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            // 指定顶点/片元着色器入口
            #pragma vertex vert
            #pragma fragment frag

            // 如果需要雾效等，启用
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // 1) 顶点数据结构 (appdata)
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // 2) 顶点着色器输出给片元着色器的结构 (v2f)
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // 3) 声明属性对应的 Shader 变量
            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            float _HoleRadius;
            float4 _HoleCenter;  // 只用 .xy
            float _HoleFeather;

            // 4) 顶点着色器：计算模型坐标->裁剪空间，以及传递纹理坐标
            v2f vert (appdata v)
            {
                v2f o;
                // Unity 提供的将顶点坐标从本地转到裁剪空间的函数
                o.vertex = UnityObjectToClipPos(v.vertex);

                // 如果还需要采样贴图(可选)，这里做UV变换
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // 如果要支持雾，就要传递雾坐标
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // 5) 片元着色器：核心的挖孔叠加逻辑
            fixed4 frag (v2f i) : SV_Target
            {
                // (可选) 获取贴图颜色
                // fixed4 col = tex2D(_MainTex, i.uv);

                // 计算像素到孔中心的距离
                float dist = distance(i.uv, _HoleCenter.xy);

                // 使用 smoothstep 做柔边过渡：
                //   当 dist < (_HoleRadius - _HoleFeather) 时，alpha约为0（完全透明）
                //   当 dist > _HoleRadius 时，alpha约为1（全不透明）
                //   中间区域则是平滑渐变
                float alpha = smoothstep(_HoleRadius - _HoleFeather, _HoleRadius, dist);

                // 根据算出的 alpha，组合叠加色并决定透明度
                fixed4 finalColor = fixed4(_Color.rgb, _Color.a * alpha);

                // 如果使用雾，需要将雾应用到finalColor上
                UNITY_APPLY_FOG(i.fogCoord, finalColor);

                // 返回最终颜色（可直接返回叠加色，也可与col做混合）
                return finalColor;
            }
            ENDCG
        }
    }
}
