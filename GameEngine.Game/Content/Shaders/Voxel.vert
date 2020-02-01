#version 450
layout(set = 0, binding = 0) uniform ViewProjBuffer
{
    mat4 View;
    mat4 Projection;
};
layout(set = 0, binding = 1) uniform WorldBuffer
{
    mat4 World;
};
layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in uint MaterialId;

layout(location = 0) out vec3 fsin_WorldPosition;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) flat out uint fsin_MaterialId;

void main()
{
    vec4 worldPos = World * vec4(Position, 1);
    gl_Position = Projection * View * worldPos;
    
    fsin_WorldPosition = worldPos.xyz / worldPos.w;
    fsin_Normal = vec3(World * vec4(Normal, 0));
    fsin_MaterialId = MaterialId;
}