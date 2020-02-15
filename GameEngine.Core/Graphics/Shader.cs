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
        internal Veldrid.Shader[] Shaders { get; }
        internal VertexElementDescription[] VertexElements { get; }

        internal Shader(Veldrid.Shader[] shaders, SpirvReflection reflection)
        {
            Shaders = shaders;

            if (reflection != null)
            {
                VertexElements = reflection.VertexElements;
            }
            else
            {
                VertexElements = new VertexElementDescription[0];
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
