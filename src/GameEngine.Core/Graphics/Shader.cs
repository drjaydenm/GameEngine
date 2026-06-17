using GameEngine.Core.Content;

namespace GameEngine.Core.Graphics;

/// <summary>
/// Contains data for a Shader. Use the <see cref="IShaderCompiler"/> to create an instance
/// </summary>
public class Shader : IContent
{
    internal IShader[] Shaders { get; }
    internal VertexElementDescription[] VertexElements { get; }
    internal ResourceLayoutDescription[] ResourceLayouts { get; }
    internal ShaderConfig Config { get; }

    public Shader(IShader[] shaders, VertexElementDescription[] vertexElements,
        ResourceLayoutDescription[] resourceLayouts, ShaderConfig config)
    {
        Shaders = shaders;
        Config = config;

        VertexElements = vertexElements;
        ResourceLayouts = resourceLayouts;
    }

    public void Dispose()
    {
        for (var i = 0; i < Shaders.Length; i++)
        {
            Shaders[i].Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
