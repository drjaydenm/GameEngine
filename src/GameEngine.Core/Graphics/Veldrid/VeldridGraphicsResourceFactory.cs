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

    public IPipeline CreateGraphicsPipeline(GraphicsPipelineDescription description)
    {
        var resourceLayouts = new ResourceLayout[description.ResourceLayouts.Length];
        for (var i = 0; i < resourceLayouts.Length; i++)
        {
            resourceLayouts[i] = ((VeldridResourceLayout)description.ResourceLayouts[i]).UnderlyingResourceLayout;
        }

        var desc = new global::Veldrid.GraphicsPipelineDescription
        {
            BlendState = VeldridUtils.ConvertBlendStateToVeldrid(description.BlendState),
            DepthStencilState = VeldridUtils.ConvertDepthStencilStateToVeldrid(description.DepthStencilState),
            RasterizerState = VeldridUtils.ConvertRasterizerStateToVeldrid(description.RasterizerState),
            PrimitiveTopology = VeldridUtils.ConvertPrimitiveTypeToVeldrid(description.PrimitiveType),
            ShaderSet = VeldridUtils.ConvertShaderSetToVeldrid(description.ShaderSet),
            ResourceLayouts = resourceLayouts,
            Outputs = VeldridUtils.ConvertOutputToVeldrid(description.Output),
            ResourceBindingModel = ResourceBindingModel.Improved
        };
        return new VeldridPipeline(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(desc));
    }

    public IResourceLayout CreateResourceLayout(ResourceLayoutDescription description)
    {
        var elements = new global::Veldrid.ResourceLayoutElementDescription[description.Elements.Length];
        for (var i = 0; i < description.Elements.Length; i++)
        {
            elements[i] = new global::Veldrid.ResourceLayoutElementDescription
            {
                Name = description.Elements[i].Name,
                Kind = (ResourceKind)description.Elements[i].Type,
                Stages = (ShaderStages)description.Elements[i].Stages
            };
        }

        var desc = new global::Veldrid.ResourceLayoutDescription
        {
            Elements = elements
        };

        return new VeldridResourceLayout(graphicsDevice.ResourceFactory.CreateResourceLayout(desc));
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        var resources = new BindableResource[description.Resources.Length];
        for (var i = 0; i < description.Resources.Length; i++)
        {
            resources[i] = description.Resources[i] switch
            {
                VeldridTexture texture => texture.UnderlyingTexture,
                VeldridSampler sampler => sampler.UnderlyingSampler,
                VeldridBuffer buffer => buffer.UnderlyingBuffer,
                _ => throw new InvalidOperationException("Unsupported resource type")
            };
        }

        var desc = new global::Veldrid.ResourceSetDescription
        {
            Layout = ((VeldridResourceLayout)description.Layout).UnderlyingResourceLayout,
            BoundResources = resources
        };

        return new VeldridResourceSet(graphicsDevice.ResourceFactory.CreateResourceSet(desc));
    }

    public global::Veldrid.Shader CreateShader(ShaderDescription description)
    {
        return graphicsDevice.ResourceFactory.CreateShader(description);
    }

    public ITexture CreateTexture(TextureDescription description)
    {
        return new VeldridTexture(graphicsDevice.ResourceFactory.CreateTexture(description));
    }
}
