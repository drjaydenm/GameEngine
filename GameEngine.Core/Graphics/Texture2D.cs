using GameEngine.Core.Content;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class Texture2D : IContent
    {
        public int Width => (int)Texture.Width;
        public int Height => (int)Texture.Width;

        internal Texture Texture { get; }

        internal Texture2D(Texture texture)
        {
            Texture = texture;
        }

        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}
