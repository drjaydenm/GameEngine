using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridCommandList(CommandList commandList) : ICommandList
{
    internal CommandList UnderlyingCommandList => commandList;

    public void Dispose()
    {
        commandList.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Begin()
    {
        commandList.Begin();
    }

    public void End()
    {
        commandList.End();
    }

    public void SetPipeline(IPipeline pipeline)
    {
        commandList.SetPipeline(((VeldridPipeline)pipeline).UnderlyingPipeline);
    }

    public void ClearColorTarget(uint index, Color color)
    {
        commandList.ClearColorTarget(index, new RgbaFloat(color.R, color.G, color.B, color.A));
    }

    public void ClearDepthStencil(float depth)
    {
        commandList.ClearDepthStencil(depth);
    }

    public void SetFramebuffer(IFramebuffer framebuffer)
    {
        commandList.SetFramebuffer(((VeldridFramebuffer)framebuffer).UnderlyingFramebuffer);
    }

    public void SetVertexBuffer(uint slot, IBuffer vertexBuffer)
    {
        commandList.SetVertexBuffer(slot, ((VeldridBuffer)vertexBuffer).UnderlyingBuffer);
    }

    public void SetIndexBuffer(IBuffer indexBuffer, IndexFormat format)
    {
        commandList.SetIndexBuffer(((VeldridBuffer)indexBuffer).UnderlyingBuffer, format == IndexFormat.UInt32 ?
            global::Veldrid.IndexFormat.UInt32 : global::Veldrid.IndexFormat.UInt16);
    }

    public void SetGraphicsResourceSet(uint slot, IResourceSet resourceSet)
    {
        commandList.SetGraphicsResourceSet(slot, ((VeldridResourceSet)resourceSet).UnderlyingResourceSet);
    }

    public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
    {
        commandList.Draw(vertexCount, instanceCount, vertexStart, instanceStart);
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
    {
        commandList.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
    }

    public void UpdateBuffer(IBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
    {
        commandList.UpdateBuffer(((VeldridBuffer)buffer).UnderlyingBuffer, bufferOffsetInBytes, source, sizeInBytes);
    }

    public void UpdateBuffer<T>(IBuffer buffer, uint bufferOffsetInBytes, T[] data) where T : unmanaged
    {
        commandList.UpdateBuffer(((VeldridBuffer)buffer).UnderlyingBuffer, bufferOffsetInBytes, data);
    }

    public void UpdateBuffer<T>(IBuffer buffer, uint bufferOffsetInBytes, T data) where T : unmanaged
    {
        commandList.UpdateBuffer(((VeldridBuffer)buffer).UnderlyingBuffer, bufferOffsetInBytes, data);
    }
}
