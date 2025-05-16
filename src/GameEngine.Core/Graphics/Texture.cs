using GameEngine.Core.Content;

namespace GameEngine.Core.Graphics
{
    public class Texture : IContent
    {
        public int Width => (int)NativeTexture.Width;
        public int Height => (int)NativeTexture.Width;

        internal Veldrid.Texture NativeTexture { get; }

        internal Texture(Veldrid.Texture texture)
        {
            NativeTexture = texture;
        }

        public void Dispose()
        {
            NativeTexture.Dispose();
        }
    }
}
