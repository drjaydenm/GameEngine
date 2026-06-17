using GameEngine.Core.Graphics;
using Veldrid;

namespace GameEngine.Graphics.Veldrid;

public class VeldridTextureView(TextureView textureView) : ITextureView
{
    internal TextureView UnderlyingTextureView => textureView;

    public void Dispose()
    {
        textureView.Dispose();
        GC.SuppressFinalize(this);
    }
}
