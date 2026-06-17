using GameEngine.Core.Graphics;
using Veldrid;

namespace GameEngine.Graphics.Veldrid;

internal class VeldridPipeline(Pipeline pipeline) : IPipeline
{
    internal Pipeline UnderlyingPipeline => pipeline;

    public void Dispose()
    {
        pipeline.Dispose();
        GC.SuppressFinalize(this);
    }
}
