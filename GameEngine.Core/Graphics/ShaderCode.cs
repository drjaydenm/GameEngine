namespace GameEngine.Core.Graphics
{
    public static class ShaderCode
    {
        public const string VertexCode = @"
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
}";

        public const string FragmentCode = @"
#version 450
layout(set = 1, binding = 0) uniform CameraBuffer
{
    vec3 CameraDirection;
    float _padding1;
    vec3 CameraPosition;
    float _padding2;
};
layout(set = 1, binding = 1) uniform LightingBuffer
{
    vec3 LightDirection;
    float _padding3;
    vec4 LightColor;
    float LightIntensity;
    float _padding4;
    float _padding5;
    float _padding6;
    vec4 AmbientLight;
};
layout(set = 2, binding = 0) uniform MaterialBuffer
{
    vec4 SpecularColor;
    float Shininess;
    float _padding7;
    float _padding8;
    float _padding9;
};
layout(set = 2, binding = 1) uniform MaterialColorBuffer
{
    vec4[4] MaterialColors;
};

layout(location = 0) in vec3 fsin_WorldPosition;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) flat in uint fsin_MaterialId;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 surfaceToLight = -LightDirection;
    vec4 diffuseColor = MaterialColors[fsin_MaterialId];
    vec3 viewDirection = normalize(CameraPosition - fsin_WorldPosition);

    vec4 ambient = AmbientLight * diffuseColor;

    float diffuseCoefficient = max(dot(fsin_Normal, surfaceToLight), 0.0);
    vec4 diffuse = diffuseCoefficient * diffuseColor * LightColor * LightIntensity;

    float specularCoefficient = 0.0;
    if (diffuseCoefficient > 0.0)
    {
        vec3 halfVector = normalize(surfaceToLight + viewDirection);
        float halfNormalDot = max(dot(halfVector, fsin_Normal), 0.0);
        specularCoefficient = pow(halfNormalDot, Shininess);
    }

    vec4 specular = specularCoefficient * SpecularColor * LightColor * LightIntensity;
    
    fsout_Color = ambient + diffuse + specular;
}";

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
