using GameEngine.Core.Graphics;
using Veldrid;

namespace GameEngine.Graphics.Veldrid;

internal class VeldridResourceSet(ResourceSet resourceSet) : IResourceSet
{
    internal ResourceSet UnderlyingResourceSet => resourceSet;

    public void Dispose()
    {
        resourceSet.Dispose();
        GC.SuppressFinalize(this);
    }
}
