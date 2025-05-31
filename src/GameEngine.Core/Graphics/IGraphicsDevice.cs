using Veldrid;

namespace GameEngine.Core.Graphics;

public interface IGraphicsDevice
{
    GraphicsBackend BackendType { get; }
    IGraphicsResourceFactory ResourceFactory { get; }
    Framebuffer SwapchainFramebuffer { get; }
    Sampler LinearSampler { get; }
    Sampler PointSampler { get; }

    void ResizeMainWindow(uint width, uint height);
    void SubmitCommands(CommandList commandList);
    void SwapBuffers();
    void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : unmanaged;
    void UpdateTexture(global::Veldrid.Texture texture,
        IntPtr source, uint sizeInBytes,
        uint x, uint y, uint z,
        uint width, uint height, uint depth,
        uint mipLevel, uint arrayLayer);
    void WaitForIdle();
}
