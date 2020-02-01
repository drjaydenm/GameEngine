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
    }
}
