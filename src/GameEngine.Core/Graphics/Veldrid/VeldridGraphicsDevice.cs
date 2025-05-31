using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridGraphicsDevice : IGraphicsDevice
{
    public GraphicsBackend BackendType => _graphicsDevice.BackendType;
    public IGraphicsResourceFactory ResourceFactory { get; }
    public Framebuffer SwapchainFramebuffer => _graphicsDevice.SwapchainFramebuffer;
    public Sampler LinearSampler => _graphicsDevice.LinearSampler;
    public Sampler PointSampler => _graphicsDevice.PointSampler;

    internal GraphicsDevice UnderlyingGraphicsDevice => _graphicsDevice;

    private readonly GraphicsDevice _graphicsDevice;

    public VeldridGraphicsDevice(Sdl2Window window, GraphicsBackend backend)
    {
        var options = new GraphicsDeviceOptions(
            debug: false,
            swapchainDepthFormat: PixelFormat.R16_UNorm,
            syncToVerticalBlank: false,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true
        );
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options, backend);

        ResourceFactory = new VeldridGraphicsResourceFactory(_graphicsDevice);
    }

    public void ResizeMainWindow(uint width, uint height)
    {
        _graphicsDevice.ResizeMainWindow(width, height);
    }

    public void SubmitCommands(ICommandList commandList)
    {
        _graphicsDevice.SubmitCommands(((VeldridCommandList)commandList).UnderlyingCommandList);
    }

    public void SwapBuffers()
    {
        _graphicsDevice.SwapBuffers();
    }

    public void UpdateBuffer<T>(IBuffer buffer, uint bufferOffsetInBytes, T[] source) where T : unmanaged
    {
        _graphicsDevice.UpdateBuffer(((VeldridBuffer)buffer).UnderlyingBuffer, bufferOffsetInBytes, source);
    }

    public void UpdateTexture(global::Veldrid.Texture texture, IntPtr source, uint sizeInBytes,
        uint x, uint y, uint z, uint width, uint height, uint depth, uint mipLevel, uint arrayLayer)
    {
        _graphicsDevice.UpdateTexture(texture, source, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
    }

    public void WaitForIdle()
    {
        _graphicsDevice.WaitForIdle();
    }

    public void Dispose()
    {
        _graphicsDevice.Dispose();
        GC.SuppressFinalize(this);
    }
}
