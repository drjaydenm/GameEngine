namespace GameEngine.Core.Graphics;

public interface IGraphicsResourceFactory
{
    IBuffer CreateBuffer(BufferDescription bufferDescription);
    ICommandList CreateCommandList();
    IFramebuffer CreateFramebuffer(FramebufferDescription framebufferDescription);
    IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription description);
    IResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    IResourceSet CreateResourceSet(ResourceSetDescription description);
    IShader CreateShader(ShaderDescription description);
    ITexture CreateTexture(TextureDescription description);
    ITextureView CreateTextureView(ITexture texture);
}
