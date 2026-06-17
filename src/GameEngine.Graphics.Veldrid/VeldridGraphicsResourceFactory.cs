using GameEngine.Core.Graphics;
using Veldrid;
using BindableResource = Veldrid.BindableResource;
using BufferDescription = GameEngine.Core.Graphics.BufferDescription;
using FramebufferAttachmentDescription = Veldrid.FramebufferAttachmentDescription;
using FramebufferDescription = GameEngine.Core.Graphics.FramebufferDescription;
using GraphicsPipelineDescription = GameEngine.Core.Graphics.GraphicsPipelineDescription;
using ResourceBindingModel = Veldrid.ResourceBindingModel;
using ResourceKind = Veldrid.ResourceKind;
using ResourceLayout = Veldrid.ResourceLayout;
using ResourceLayoutDescription = GameEngine.Core.Graphics.ResourceLayoutDescription;
using ResourceSetDescription = GameEngine.Core.Graphics.ResourceSetDescription;
using ShaderDescription = GameEngine.Core.Graphics.ShaderDescription;
using ShaderStages = Veldrid.ShaderStages;
using TextureDescription = GameEngine.Core.Graphics.TextureDescription;

namespace GameEngine.Graphics.Veldrid;

internal class VeldridGraphicsResourceFactory(global::Veldrid.GraphicsDevice graphicsDevice) : IGraphicsResourceFactory
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

    public IFramebuffer CreateFramebuffer(FramebufferDescription framebufferDescription)
    {
        var colorTargets = new FramebufferAttachmentDescription[framebufferDescription.ColorTargets.Length];
        for (var i = 0; i < colorTargets.Length; i++)
        {
            colorTargets[i] = new FramebufferAttachmentDescription
            {
                Target = ((VeldridTexture)framebufferDescription.ColorTargets[i].Target).UnderlyingTexture,
                ArrayLayer = framebufferDescription.ColorTargets[i].ArrayLayer,
                MipLevel = framebufferDescription.ColorTargets[i].MipLevel
            };
        }

        var desc = new global::Veldrid.FramebufferDescription
        {
            DepthTarget = framebufferDescription.DepthTarget.HasValue
                ? new FramebufferAttachmentDescription
                {
                    Target = ((VeldridTexture)framebufferDescription.DepthTarget.Value.Target).UnderlyingTexture,
                    ArrayLayer = framebufferDescription.DepthTarget.Value.ArrayLayer,
                    MipLevel = framebufferDescription.DepthTarget.Value.MipLevel
                }
                : null,
            ColorTargets = colorTargets
        };

        return new VeldridFramebuffer(graphicsDevice.ResourceFactory.CreateFramebuffer(desc));
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
                VeldridTextureView textureView => textureView.UnderlyingTextureView,
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

    public IShader CreateShader(ShaderDescription description)
    {
        var desc = new global::Veldrid.ShaderDescription
        {
            Stage = (ShaderStages)description.Stage,
            EntryPoint = description.EntryPoint,
            ShaderBytes = description.ShaderBytes,
            Debug = description.Debug
        };

        return new VeldridShader(graphicsDevice.ResourceFactory.CreateShader(desc));
    }

    public ITexture CreateTexture(TextureDescription description)
    {
        var desc = new global::Veldrid.TextureDescription
        {
            Width = description.Width,
            Height = description.Height,
            Depth = description.Depth,
            MipLevels = description.MipLevels,
            ArrayLayers = description.ArrayLayers,
            Format = VeldridUtils.ConvertPixelFormatToVeldrid(description.Format),
            Usage = (global::Veldrid.TextureUsage)description.Usage,
            Type = VeldridUtils.ConvertTextureTypeToVeldrid(description.Type),
            SampleCount = VeldridUtils.ConvertTextureSampleCountToVeldrid(description.SampleCount)
        };

        return new VeldridTexture(graphicsDevice.ResourceFactory.CreateTexture(desc));
    }

    public ITextureView CreateTextureView(ITexture texture)
    {
        return texture is not VeldridTexture veldridTexture
            ? throw new InvalidOperationException("Texture is not a VeldridTexture")
            : new VeldridTextureView(graphicsDevice.ResourceFactory.CreateTextureView(veldridTexture.UnderlyingTexture));
    }
}
