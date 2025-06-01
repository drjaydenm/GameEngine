namespace GameEngine.Core.Graphics;

public interface IGraphicsResourceFactory
{
    IBuffer CreateBuffer(BufferDescription bufferDescription);
    ICommandList CreateCommandList();
    IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription description);
    IResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    IResourceSet CreateResourceSet(ResourceSetDescription description);
    global::Veldrid.Shader CreateShader(global::Veldrid.ShaderDescription description);
    ITexture CreateTexture(global::Veldrid.TextureDescription description);
}
