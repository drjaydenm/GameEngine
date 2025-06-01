using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridGraphicsDevice : IGraphicsDevice
{
    public GraphicsBackend BackendType => VeldridUtils.ConvertGraphicsBackendFromVeldrid(_graphicsDevice.BackendType);
    public IGraphicsResourceFactory ResourceFactory { get; }
    public IFramebuffer SwapchainFramebuffer => _swapchainFramebuffer;
    public ISampler LinearSampler => _linearSampler;
    public ISampler PointSampler => _pointSampler;

    internal GraphicsDevice UnderlyingGraphicsDevice => _graphicsDevice;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ISampler _linearSampler;
    private readonly ISampler _pointSampler;
    private readonly IFramebuffer _swapchainFramebuffer;

    public VeldridGraphicsDevice(Sdl2Window window, GraphicsBackend backend)
    {
        var options = new GraphicsDeviceOptions(
            debug: false,
            swapchainDepthFormat: global::Veldrid.PixelFormat.R16_UNorm,
            syncToVerticalBlank: false,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true
        );
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options,
            VeldridUtils.ConvertGraphicsBackendToVeldrid(backend));

        ResourceFactory = new VeldridGraphicsResourceFactory(_graphicsDevice);

        _linearSampler = new VeldridSampler(_graphicsDevice.LinearSampler);
        _pointSampler = new VeldridSampler(_graphicsDevice.PointSampler);
        _swapchainFramebuffer = new VeldridFramebuffer(_graphicsDevice.SwapchainFramebuffer);
    }

    public void Dispose()
    {
        _graphicsDevice.Dispose();
        GC.SuppressFinalize(this);
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

    public void UpdateTexture(ITexture texture, IntPtr source, uint sizeInBytes,
        uint x, uint y, uint z, uint width, uint height, uint depth, uint mipLevel, uint arrayLayer)
    {
        _graphicsDevice.UpdateTexture(((VeldridTexture)texture).UnderlyingTexture,
            source, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
    }

    public void WaitForIdle()
    {
        _graphicsDevice.WaitForIdle();
    }
}
