namespace GameEngine.Core.Graphics
{
    public static class ShaderCode
    {
        public const string DebugDrawVertexCode = @"
#version 450
layout(set = 0, binding = 0) uniform WVPBuffer
{
    mat4 WVP;
};
layout(set = 0, binding = 1) uniform ColorBuffer
{
    vec4 Color;
};
layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 VertColor;

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = WVP * vec4(Position, 1);
    fsin_Color = Color;
}";

        public const string DebugDrawFragmentCode = @"
#version 450
layout(location = 0) in vec4 fsin_Color;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

        public const string SkyboxVertexCode = @"
#version 450
layout(set = 0, binding = 0) uniform ViewProjBuffer
{
    mat4 View;
    mat4 Projection;
};
layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;

layout(location = 0) out vec3 fsin_TexCoord;

void main()
{
    vec4 pos = Projection * View * vec4(Position, 1.0);
    gl_Position = pos.xyww;

    fsin_TexCoord = Position;
}";

        public const string SkyboxFragmentCode = @"
#version 450
layout(set = 1, binding = 0) uniform SkyboxParameters
{
    vec4 SkyColor;
    vec4 SunColor;
    vec3 SunDirection;
    float _padding1;
};
layout(location = 0) in vec3 fsin_TexCoord;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 normal = normalize(fsin_TexCoord);
    float NdotSun = clamp(dot(normal, -normalize(SunDirection)), 0, 1);
    float sunPower = pow(NdotSun, 100);

    fsout_Color = SkyColor + (SunColor * sunPower);
}
";
    }
}
