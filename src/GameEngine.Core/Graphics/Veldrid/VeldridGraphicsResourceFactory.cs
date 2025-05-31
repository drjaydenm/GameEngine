using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

public class VeldridGraphicsResourceFactory(GraphicsDevice graphicsDevice) : IGraphicsResourceFactory
{
    public DeviceBuffer CreateBuffer(BufferDescription bufferDescription)
    {
        return graphicsDevice.ResourceFactory.CreateBuffer(bufferDescription);
    }

    public CommandList CreateCommandList()
    {
        return graphicsDevice.ResourceFactory.CreateCommandList();
    }

    public Pipeline CreateGraphicsPipeline(GraphicsPipelineDescription description)
    {
        return graphicsDevice.ResourceFactory.CreateGraphicsPipeline(description);
    }

    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description)
    {
        return graphicsDevice.ResourceFactory.CreateResourceLayout(description);
    }

    public ResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        return graphicsDevice.ResourceFactory.CreateResourceSet(description);
    }

    public global::Veldrid.Shader CreateShader(ShaderDescription description)
    {
        return graphicsDevice.ResourceFactory.CreateShader(description);
    }

    public global::Veldrid.Texture CreateTexture(TextureDescription description)
    {
        return graphicsDevice.ResourceFactory.CreateTexture(description);
    }
}
