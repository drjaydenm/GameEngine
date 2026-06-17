using GameEngine.Core.Graphics;
using Veldrid;

namespace GameEngine.Graphics.Veldrid;

internal class VeldridTexture(Texture texture) : ITexture
{
    internal Texture UnderlyingTexture => texture;

    public void Dispose()
    {
        texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
