using GameEngine.Core.Content;
using Veldrid;
using Veldrid.SPIRV;

namespace GameEngine.Core.Graphics
{
    /// <summary>
    /// Contains data for a Shader. Use the <c>ShaderCompiler</c> to create an instance
    /// </summary>
    public class Shader : IContent
    {
        internal global::Veldrid.Shader[] Shaders { get; }
        internal VertexElementDescription[] VertexElements { get; }
        internal ResourceLayoutDescription[] ResourceLayouts { get; }
        internal ShaderConfig Config { get; }

        internal Shader(global::Veldrid.Shader[] shaders, SpirvReflection reflection, ShaderConfig config)
        {
            Shaders = shaders;
            Config = config;

            if (reflection != null)
            {
                VertexElements = reflection.VertexElements;
                ResourceLayouts = reflection.ResourceLayouts;
            }
            else
            {
                VertexElements = new VertexElementDescription[0];
                ResourceLayouts = new ResourceLayoutDescription[0];
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < Shaders.Length; i++)
            {
                Shaders[i].Dispose();
            }
        }
    }
}
