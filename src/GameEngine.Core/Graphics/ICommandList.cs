namespace GameEngine.Core.Graphics;

public interface ICommandList : IDisposable
{
    // Recording control
    void Begin();
    void End();

    // Pipeline state
    void SetPipeline(IPipeline pipeline);

    // Framebuffer
    void ClearColorTarget(uint index, Color color);
    void ClearDepthStencil(float depth);
    void SetFramebuffer(IFramebuffer framebuffer);

    // Resource binding
    void SetVertexBuffer(uint slot, IBuffer vertexBuffer);
    void SetIndexBuffer(IBuffer indexBuffer, IndexFormat format);
    void SetGraphicsResourceSet(uint slot, IResourceSet resourceSet);

    // Drawing commands
    void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart);
    void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart);

    // Buffer management
    void UpdateBuffer(IBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes);
    void UpdateBuffer<T>(IBuffer buffer, uint bufferOffsetInBytes, T[] data) where T : unmanaged;
    void UpdateBuffer<T>(IBuffer buffer, uint bufferOffsetInBytes, T data) where T : unmanaged;
}
