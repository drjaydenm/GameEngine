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
    vec4 FogColor;
    float FogStartDistance;
    float _padding7;
    float _padding8;
    float _padding9;
    float FogEndDistance;
    float _padding10;
    float _padding11;
    float _padding12;
};
layout(set = 2, binding = 0) uniform MaterialBuffer
{
    vec4 SpecularColor;
    float Shininess;
    float _padding13;
    float _padding14;
    float _padding15;
};
layout(set = 3, binding = 0) uniform texture2DArray Texture;
layout(set = 3, binding = 1) uniform sampler TextureSampler;

layout(location = 0) in vec3 fsin_WorldPosition;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in vec2 fsin_TexCoord;
layout(location = 3) flat in uint fsin_MaterialId;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    vec3 surfaceToLight = -LightDirection;
    vec3 cameraToFragment = CameraPosition - fsin_WorldPosition;
    vec3 viewDirection = normalize(cameraToFragment);

    vec4 diffuseColor = texture(sampler2DArray(Texture, TextureSampler), vec3(fsin_TexCoord, fsin_MaterialId));

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

    float fogDistance = length(cameraToFragment);
    float fogStrength = 1.0 - clamp((FogEndDistance - fogDistance) / (FogEndDistance - FogStartDistance), 0.0, 1.0);
    vec4 fog = FogColor * fogStrength;
    
    fsout_Color = ((ambient + diffuse + specular) * (1.0 - fogStrength)) + fog;
}