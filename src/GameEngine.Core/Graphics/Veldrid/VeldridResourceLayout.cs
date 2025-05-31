using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridResourceLayout(ResourceLayout resourceLayout) : IResourceLayout
{
    internal ResourceLayout UnderlyingResourceLayout => resourceLayout;

    public void Dispose()
    {
        resourceLayout.Dispose();
        GC.SuppressFinalize(this);
    }
}
