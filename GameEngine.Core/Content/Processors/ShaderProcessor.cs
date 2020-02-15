using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;

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
            return ShaderCompiler.CompileShader(engine, contentRaw.VertexShader, contentRaw.FragmentShader);
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((ShaderRaw)contentRaw);
        }
    }
}
