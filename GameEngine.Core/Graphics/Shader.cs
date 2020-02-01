using GameEngine.Core.Content;

namespace GameEngine.Core.Graphics
{
    public class Shader : IContent
    {
        public Veldrid.Shader[] Shaders { get; }

        public Shader(Veldrid.Shader[] shaders)
        {
            Shaders = shaders;
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
