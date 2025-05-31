using Veldrid;

namespace GameEngine.Core.Graphics;

public interface IGraphicsResourceFactory
{
    DeviceBuffer CreateBuffer(BufferDescription bufferDescription);
    CommandList CreateCommandList();
    Pipeline CreateGraphicsPipeline(GraphicsPipelineDescription description);
    ResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    ResourceSet CreateResourceSet(ResourceSetDescription description);
    global::Veldrid.Shader CreateShader(ShaderDescription description);
    global::Veldrid.Texture CreateTexture(TextureDescription description);
}
