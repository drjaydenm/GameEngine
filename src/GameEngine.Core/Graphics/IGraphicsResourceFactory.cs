using Veldrid;

namespace GameEngine.Core.Graphics;

public interface IGraphicsResourceFactory
{
    IBuffer CreateBuffer(BufferDescription bufferDescription);
    ICommandList CreateCommandList();
    Pipeline CreateGraphicsPipeline(GraphicsPipelineDescription description);
    IResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    IResourceSet CreateResourceSet(ResourceSetDescription description);
    global::Veldrid.Shader CreateShader(ShaderDescription description);
    ITexture CreateTexture(TextureDescription description);
}
