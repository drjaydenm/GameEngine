using GameEngine.Core.Graphics;
using Veldrid;
using OutputAttachmentDescription = GameEngine.Core.Graphics.OutputAttachmentDescription;
using OutputDescription = GameEngine.Core.Graphics.OutputDescription;

namespace GameEngine.Graphics.Veldrid;

public class VeldridFramebuffer : IFramebuffer
{
    public OutputDescription OutputDescription { get; }
    public uint Width { get; }
    public uint Height { get; }

    internal Framebuffer UnderlyingFramebuffer => _framebuffer;

    private readonly Framebuffer _framebuffer;

    public VeldridFramebuffer(Framebuffer framebuffer)
    {
        _framebuffer = framebuffer;

        // Store a copy of the output description
        var colorAttachments = new OutputAttachmentDescription[_framebuffer.OutputDescription.ColorAttachments.Length];
        for (var i = 0; i < _framebuffer.OutputDescription.ColorAttachments.Length; i++)
        {
            colorAttachments[i] = new OutputAttachmentDescription(VeldridUtils.ConvertPixelFormatFromVeldrid(
                _framebuffer.OutputDescription.ColorAttachments[i].Format));
        }
        OutputDescription = new OutputDescription
        {
            DepthAttachment = _framebuffer.OutputDescription.DepthAttachment.HasValue
                ? new OutputAttachmentDescription(VeldridUtils.ConvertPixelFormatFromVeldrid(
                    _framebuffer.OutputDescription.DepthAttachment.Value.Format))
                : null,
            ColorAttachments = colorAttachments,
            SampleCount = VeldridUtils.ConvertTextureSampleCountFromVeldrid(_framebuffer.OutputDescription.SampleCount)
        };

        Width = framebuffer.Width;
        Height = framebuffer.Height;
    }

    public void Dispose()
    {
        _framebuffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
