using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridGraphicsResourceFactory(GraphicsDevice graphicsDevice) : IGraphicsResourceFactory
{
    public IBuffer CreateBuffer(BufferDescription bufferDescription)
    {
        var desc = new global::Veldrid.BufferDescription
        {
            SizeInBytes = bufferDescription.SizeInBytes,
            Usage = (global::Veldrid.BufferUsage)bufferDescription.Usage,
            StructureByteStride = bufferDescription.StructureByteStride,
            RawBuffer = bufferDescription.IsRawBuffer
        };

        return new VeldridBuffer(graphicsDevice.ResourceFactory.CreateBuffer(desc));
    }

    public ICommandList CreateCommandList()
    {
        return new VeldridCommandList(graphicsDevice.ResourceFactory.CreateCommandList());
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
