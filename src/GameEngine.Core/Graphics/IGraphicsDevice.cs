using Veldrid;

namespace GameEngine.Core.Graphics;

public interface IGraphicsDevice : IDisposable
{
    GraphicsBackend BackendType { get; }
    IGraphicsResourceFactory ResourceFactory { get; }
    Framebuffer SwapchainFramebuffer { get; }
    ISampler LinearSampler { get; }
    ISampler PointSampler { get; }

    void ResizeMainWindow(uint width, uint height);
    void SubmitCommands(ICommandList commandList);
    void SwapBuffers();
    void UpdateBuffer<T>(IBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : unmanaged;
    void UpdateTexture(ITexture texture,
        IntPtr source, uint sizeInBytes,
        uint x, uint y, uint z,
        uint width, uint height, uint depth,
        uint mipLevel, uint arrayLayer);
    void WaitForIdle();
}
