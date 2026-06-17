using GameEngine.Core.Graphics;
using Veldrid;

namespace GameEngine.Graphics.Veldrid;

internal class VeldridSampler(Sampler sampler) : ISampler
{
    internal Sampler UnderlyingSampler => sampler;

    public void Dispose()
    {
        sampler.Dispose();
        GC.SuppressFinalize(this);
    }
}
