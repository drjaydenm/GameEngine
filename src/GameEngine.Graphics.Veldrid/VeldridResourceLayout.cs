using GameEngine.Core.Graphics;
using Veldrid;

namespace GameEngine.Graphics.Veldrid;

internal class VeldridResourceLayout(ResourceLayout resourceLayout) : IResourceLayout
{
    internal ResourceLayout UnderlyingResourceLayout => resourceLayout;

    public void Dispose()
    {
        resourceLayout.Dispose();
        GC.SuppressFinalize(this);
    }
}
