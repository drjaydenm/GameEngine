using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridPipeline(Pipeline pipeline) : IPipeline
{
    internal Pipeline UnderlyingPipeline => pipeline;

    public void Dispose()
    {
        pipeline.Dispose();
        GC.SuppressFinalize(this);
    }
}
