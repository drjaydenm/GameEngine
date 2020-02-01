using System.Text;
using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;
using Veldrid.SPIRV;

namespace GameEngine.Core.Content.Processors
{
    public class ShaderProcessor : IContentProcessor<ShaderRaw, Shader>
    {
        private readonly Engine engine;

        public ShaderProcessor(Engine engine)
        {
            this.engine = engine;
        }

        public Shader Process(ShaderRaw contentRaw)
        {
            var vertexShaderDesc = new Veldrid.ShaderDescription(
                Veldrid.ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(contentRaw.VertexShader),
                "main");
            var fragmentShaderDesc = new Veldrid.ShaderDescription(
                Veldrid.ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(contentRaw.FragmentShader),
                "main");

            var factory = engine.GraphicsDevice.ResourceFactory;
            var shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            return new Shader(shaders);
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((ShaderRaw)contentRaw);
        }
    }
}
