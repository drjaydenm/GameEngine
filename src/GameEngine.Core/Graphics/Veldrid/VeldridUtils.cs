using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

internal static class VeldridUtils
{
    internal static PixelFormat ConvertPixelFormatFromVeldrid(global::Veldrid.PixelFormat format)
    {
        return format switch
        {
            global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm => PixelFormat.B8_G8_R8_A8_UNorm,
            global::Veldrid.PixelFormat.R8_G8_B8_A8_UNorm => PixelFormat.R8_G8_B8_A8_UNorm,
            global::Veldrid.PixelFormat.R16_UNorm => PixelFormat.R16_UNorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported pixel format")
        };
    }

    internal static global::Veldrid.PixelFormat ConvertPixelFormatToVeldrid(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.B8_G8_R8_A8_UNorm => global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm,
            PixelFormat.R8_G8_B8_A8_UNorm => global::Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
            PixelFormat.R16_UNorm => global::Veldrid.PixelFormat.R16_UNorm,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported pixel format")
        };
    }

    internal static TextureSampleCount ConvertSampleCountFromVeldrid(global::Veldrid.TextureSampleCount sampleCount)
    {
        return sampleCount switch
        {
            global::Veldrid.TextureSampleCount.Count1 => TextureSampleCount.Count1,
            global::Veldrid.TextureSampleCount.Count2 => TextureSampleCount.Count2,
            global::Veldrid.TextureSampleCount.Count4 => TextureSampleCount.Count4,
            global::Veldrid.TextureSampleCount.Count8 => TextureSampleCount.Count8,
            global::Veldrid.TextureSampleCount.Count16 => TextureSampleCount.Count16,
            global::Veldrid.TextureSampleCount.Count32 => TextureSampleCount.Count32,
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount, "Unsupported sample count")
        };
    }

    internal static global::Veldrid.TextureSampleCount ConvertSampleCountToVeldrid(TextureSampleCount sampleCount)
    {
        return sampleCount switch
        {
            TextureSampleCount.Count1 => global::Veldrid.TextureSampleCount.Count1,
            TextureSampleCount.Count2 => global::Veldrid.TextureSampleCount.Count2,
            TextureSampleCount.Count4 => global::Veldrid.TextureSampleCount.Count4,
            TextureSampleCount.Count8 => global::Veldrid.TextureSampleCount.Count8,
            TextureSampleCount.Count16 => global::Veldrid.TextureSampleCount.Count16,
            TextureSampleCount.Count32 => global::Veldrid.TextureSampleCount.Count32,
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount, "Unsupported sample count")
        };
    }

    internal static PrimitiveTopology ConvertPrimitiveTypeToVeldrid(PrimitiveType primitiveType)
    {
        return primitiveType switch
        {
            PrimitiveType.TriangleList => PrimitiveTopology.TriangleList,
            PrimitiveType.TriangleStrip => PrimitiveTopology.TriangleStrip,
            PrimitiveType.LineList => PrimitiveTopology.LineList,
            PrimitiveType.LineStrip => PrimitiveTopology.LineStrip,
            PrimitiveType.PointList => PrimitiveTopology.PointList,
            _ => throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, "Unknown primitive type")
        };
    }

    internal static RgbaFloat ConvertColorToVeldrid(Color color)
    {
        return new RgbaFloat(color.R, color.G, color.B, color.A);
    }

    internal static global::Veldrid.BlendStateDescription ConvertBlendStateToVeldrid(BlendStateDescription description)
    {
        var attachmentStates = new global::Veldrid.BlendAttachmentDescription[description.AttachmentStates.Length];
        for (var i = 0; i < description.AttachmentStates.Length; i++)
        {
            attachmentStates[i] = new global::Veldrid.BlendAttachmentDescription
            {
                BlendEnabled = description.AttachmentStates[i].BlendEnabled,
                ColorWriteMask = description.AttachmentStates[i].ColorWriteMask.HasValue
                    ? (global::Veldrid.ColorWriteMask)description.AttachmentStates[i].ColorWriteMask
                    : null,
                SourceColorFactor = ConvertBlendFactorToVeldrid(description.AttachmentStates[i].SourceColorFactor),
                DestinationColorFactor =
                    ConvertBlendFactorToVeldrid(description.AttachmentStates[i].DestinationColorFactor),
                ColorFunction = ConvertBlendFunctionToVeldrid(description.AttachmentStates[i].ColorFunction),
                SourceAlphaFactor = ConvertBlendFactorToVeldrid(description.AttachmentStates[i].SourceAlphaFactor),
                DestinationAlphaFactor =
                    ConvertBlendFactorToVeldrid(description.AttachmentStates[i].DestinationAlphaFactor),
                AlphaFunction = ConvertBlendFunctionToVeldrid(description.AttachmentStates[i].AlphaFunction)
            };
        }

        return new global::Veldrid.BlendStateDescription
        {
            BlendFactor = ConvertColorToVeldrid(description.BlendFactor),
            AlphaToCoverageEnabled = description.AlphaToCoverageEnabled,
            AttachmentStates = attachmentStates
        };
    }

    internal static global::Veldrid.BlendFactor ConvertBlendFactorToVeldrid(BlendFactor blendFactor)
    {
        return blendFactor switch
        {
            BlendFactor.Zero => global::Veldrid.BlendFactor.Zero,
            BlendFactor.One => global::Veldrid.BlendFactor.One,
            BlendFactor.SourceAlpha => global::Veldrid.BlendFactor.SourceAlpha,
            BlendFactor.InverseSourceAlpha => global::Veldrid.BlendFactor.InverseSourceAlpha,
            BlendFactor.DestinationAlpha => global::Veldrid.BlendFactor.DestinationAlpha,
            BlendFactor.InverseDestinationAlpha => global::Veldrid.BlendFactor.InverseDestinationAlpha,
            BlendFactor.SourceColor => global::Veldrid.BlendFactor.SourceColor,
            BlendFactor.InverseSourceColor => global::Veldrid.BlendFactor.InverseSourceColor,
            BlendFactor.DestinationColor => global::Veldrid.BlendFactor.DestinationColor,
            BlendFactor.InverseDestinationColor => global::Veldrid.BlendFactor.InverseDestinationColor,
            BlendFactor.BlendFactor => global::Veldrid.BlendFactor.BlendFactor,
            BlendFactor.InverseBlendFactor => global::Veldrid.BlendFactor.InverseBlendFactor,
            _ => throw new ArgumentOutOfRangeException(nameof(blendFactor), blendFactor, "Unsupported blend factor")
        };
    }

    internal static global::Veldrid.BlendFunction ConvertBlendFunctionToVeldrid(BlendFunction blendFunction)
    {
        return blendFunction switch
        {
            BlendFunction.Add => global::Veldrid.BlendFunction.Add,
            BlendFunction.Subtract => global::Veldrid.BlendFunction.Subtract,
            BlendFunction.ReverseSubtract => global::Veldrid.BlendFunction.ReverseSubtract,
            BlendFunction.Minimum => global::Veldrid.BlendFunction.Minimum,
            BlendFunction.Maximum => global::Veldrid.BlendFunction.Maximum,
            _ => throw new ArgumentOutOfRangeException(nameof(blendFunction), blendFunction, "Unsupported blend function")
        };
    }

    internal static global::Veldrid.DepthStencilStateDescription ConvertDepthStencilStateToVeldrid(
        DepthStencilStateDescription description)
    {
        return new global::Veldrid.DepthStencilStateDescription
        {
            DepthTestEnabled = description.DepthTestEnabled,
            DepthWriteEnabled = description.DepthWriteEnabled,
            DepthComparison = ConvertComparisonKindToVeldrid(description.ComparisonType),
            StencilTestEnabled = description.StencilTestEnabled,
            StencilFront = new global::Veldrid.StencilBehaviorDescription
            {
                Fail = ConvertStencilOperationToVeldrid(description.StencilFront.Fail),
                Pass = ConvertStencilOperationToVeldrid(description.StencilFront.Pass),
                DepthFail = ConvertStencilOperationToVeldrid(description.StencilFront.DepthFail),
                Comparison = ConvertComparisonKindToVeldrid(description.StencilFront.Comparison)
            },
            StencilBack = new global::Veldrid.StencilBehaviorDescription
            {
                Fail = ConvertStencilOperationToVeldrid(description.StencilBack.Fail),
                Pass = ConvertStencilOperationToVeldrid(description.StencilBack.Pass),
                DepthFail = ConvertStencilOperationToVeldrid(description.StencilBack.DepthFail),
                Comparison = ConvertComparisonKindToVeldrid(description.StencilBack.Comparison)
            },
            StencilReadMask = description.StencilReadMask,
            StencilWriteMask = description.StencilWriteMask,
            StencilReference = description.StencilReference
        };
    }

    internal static ComparisonKind ConvertComparisonKindToVeldrid(ComparisonType comparisonType)
    {
        return comparisonType switch
        {
            ComparisonType.Never => ComparisonKind.Never,
            ComparisonType.Less => ComparisonKind.Less,
            ComparisonType.Equal => ComparisonKind.Equal,
            ComparisonType.LessEqual => ComparisonKind.LessEqual,
            ComparisonType.Greater => ComparisonKind.Greater,
            ComparisonType.NotEqual => ComparisonKind.NotEqual,
            ComparisonType.GreaterEqual => ComparisonKind.GreaterEqual,
            ComparisonType.Always => ComparisonKind.Always,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, "Unsupported comparison type")
        };
    }

    internal static global::Veldrid.StencilOperation ConvertStencilOperationToVeldrid(StencilOperation stencilOperation)
    {
        return stencilOperation switch
        {
            StencilOperation.Keep => global::Veldrid.StencilOperation.Keep,
            StencilOperation.Zero => global::Veldrid.StencilOperation.Zero,
            StencilOperation.Replace => global::Veldrid.StencilOperation.Replace,
            StencilOperation.IncrementAndClamp => global::Veldrid.StencilOperation.IncrementAndClamp,
            StencilOperation.DecrementAndClamp => global::Veldrid.StencilOperation.DecrementAndClamp,
            StencilOperation.Invert => global::Veldrid.StencilOperation.Invert,
            StencilOperation.IncrementAndWrap => global::Veldrid.StencilOperation.IncrementAndWrap,
            StencilOperation.DecrementAndWrap => global::Veldrid.StencilOperation.DecrementAndWrap,
            _ => throw new ArgumentOutOfRangeException(nameof(stencilOperation), stencilOperation, "Unsupported stencil operation")
        };
    }

    internal static global::Veldrid.RasterizerStateDescription ConvertRasterizerStateToVeldrid(
        RasterizerStateDescription description)
    {
        return new global::Veldrid.RasterizerStateDescription
        {
            CullMode = ConvertFaceCullModeToVeldrid(description.CullMode),
            FillMode = ConvertPolygonFillModeToVeldrid(description.FillMode),
            FrontFace = ConvertFrontFaceToVeldrid(description.FrontFace),
            DepthClipEnabled = description.DepthClipEnabled,
            ScissorTestEnabled = description.ScissorTestEnabled
        };
    }

    internal static global::Veldrid.FaceCullMode ConvertFaceCullModeToVeldrid(FaceCullMode cullMode)
    {
        return cullMode switch
        {
            FaceCullMode.Back => global::Veldrid.FaceCullMode.Back,
            FaceCullMode.Front => global::Veldrid.FaceCullMode.Front,
            FaceCullMode.None => global::Veldrid.FaceCullMode.None,
            _ => throw new ArgumentOutOfRangeException(nameof(cullMode), cullMode, "Unsupported face cull mode")
        };
    }


    internal static global::Veldrid.PolygonFillMode ConvertPolygonFillModeToVeldrid(PolygonFillMode polygonFillMode)
    {
        return polygonFillMode switch
        {
            PolygonFillMode.Solid => global::Veldrid.PolygonFillMode.Solid,
            PolygonFillMode.Wireframe => global::Veldrid.PolygonFillMode.Wireframe,
            _ => throw new ArgumentOutOfRangeException(nameof(polygonFillMode), polygonFillMode,
                "Unsupported polygon fill mode")
        };
    }

    internal static global::Veldrid.FrontFace ConvertFrontFaceToVeldrid(FrontFace frontFace)
    {
        return frontFace switch
        {
            FrontFace.Clockwise => global::Veldrid.FrontFace.Clockwise,
            FrontFace.CounterClockwise => global::Veldrid.FrontFace.CounterClockwise,
            _ => throw new ArgumentOutOfRangeException(nameof(frontFace), frontFace, "Unsupported front face")
        };
    }

    internal static global::Veldrid.ShaderSetDescription ConvertShaderSetToVeldrid(
        ShaderSetDescription shaderSetDescription)
    {
        var vertexLayouts = new global::Veldrid.VertexLayoutDescription[shaderSetDescription.VertexLayouts.Length];
        for (var i = 0; i < shaderSetDescription.VertexLayouts.Length; i++)
        {
            vertexLayouts[i] = ConvertVertexLayoutToVeldrid(shaderSetDescription.VertexLayouts[i]);
        }

        var shaders = new global::Veldrid.Shader[shaderSetDescription.Shader.Shaders.Length];
        for (var i = 0; i < shaderSetDescription.Shader.Shaders.Length; i++)
        {
            shaders[i] = shaderSetDescription.Shader.Shaders[i];
        }

        var specializations = new SpecializationConstant[shaderSetDescription.Specializations?.Length ?? 0];
        for (var i = 0; i < shaderSetDescription.Specializations?.Length; i++)
        {
            specializations[i] = new SpecializationConstant
            {
                ID = shaderSetDescription.Specializations[i].Id,
                Type = ConvertShaderConstantTypeToVeldrid(shaderSetDescription.Specializations[i].Type),
                Data = shaderSetDescription.Specializations[i].Data
            };
        }

        return new global::Veldrid.ShaderSetDescription
        {
            VertexLayouts = vertexLayouts,
            Shaders = shaders,
            Specializations = specializations
        };
    }

    internal static global::Veldrid.VertexLayoutDescription ConvertVertexLayoutToVeldrid(
        VertexLayoutDescription vertexLayout)
    {
        var elements = new global::Veldrid.VertexElementDescription[vertexLayout.Elements.Length];
        for (var i = 0; i < vertexLayout.Elements.Length; i++)
        {
            elements[i] = ConvertVertexElementToVeldrid(vertexLayout.Elements[i]);
        }

        return new global::Veldrid.VertexLayoutDescription
        {
            Stride = vertexLayout.Stride,
            Elements = elements,
            InstanceStepRate = vertexLayout.InstanceStepRate
        };
    }

    internal static global::Veldrid.VertexElementDescription ConvertVertexElementToVeldrid(
        VertexElementDescription vertexElement)
    {
        return new global::Veldrid.VertexElementDescription
        {
            Name = vertexElement.Name,
            Semantic = VertexElementSemantic.TextureCoordinate,
            Format = ConvertVertexElementFormatToVeldrid(vertexElement.Format),
            Offset = vertexElement.Offset
        };
    }

    internal static global::Veldrid.ShaderConstantType ConvertShaderConstantTypeToVeldrid(
        ShaderConstantType shaderConstantType)
    {
        return shaderConstantType switch
        {
            ShaderConstantType.Bool => global::Veldrid.ShaderConstantType.Bool,
            ShaderConstantType.UInt16 => global::Veldrid.ShaderConstantType.UInt16,
            ShaderConstantType.Int16 => global::Veldrid.ShaderConstantType.Int16,
            ShaderConstantType.UInt32 => global::Veldrid.ShaderConstantType.UInt32,
            ShaderConstantType.Int32 => global::Veldrid.ShaderConstantType.Int32,
            ShaderConstantType.UInt64 => global::Veldrid.ShaderConstantType.UInt64,
            ShaderConstantType.Int64 => global::Veldrid.ShaderConstantType.Int64,
            ShaderConstantType.Float => global::Veldrid.ShaderConstantType.Float,
            ShaderConstantType.Double => global::Veldrid.ShaderConstantType.Double,
            _ => throw new ArgumentOutOfRangeException(nameof(shaderConstantType),
                shaderConstantType, "Unsupported shader constant type")
        };
    }

    internal static global::Veldrid.OutputDescription ConvertOutputToVeldrid(OutputDescription outputDescription)
    {
        var colorAttachments = new global::Veldrid.OutputAttachmentDescription[outputDescription.ColorAttachments.Length];
        for (var i = 0; i < outputDescription.ColorAttachments.Length; i++)
        {
            colorAttachments[i] = ConvertOutputAttachmentToVeldrid(outputDescription.ColorAttachments[i]);
        }

        return new global::Veldrid.OutputDescription
        {
            DepthAttachment = outputDescription.DepthAttachment.HasValue
                ? ConvertOutputAttachmentToVeldrid(outputDescription.DepthAttachment.Value) : null,
            ColorAttachments = colorAttachments,
            SampleCount = ConvertSampleCountToVeldrid(outputDescription.SampleCount)
        };
    }

    internal static global::Veldrid.OutputAttachmentDescription ConvertOutputAttachmentToVeldrid(
        OutputAttachmentDescription outputAttachmentDescription)
    {
        return new global::Veldrid.OutputAttachmentDescription
        {
            Format = ConvertPixelFormatToVeldrid(outputAttachmentDescription.Format)
        };
    }

    internal static VertexElementFormat ConvertVertexFormatFromVeldrid(global::Veldrid.VertexElementFormat format)
    {
        return format switch
        {
            global::Veldrid.VertexElementFormat.Float1 => VertexElementFormat.Float1,
            global::Veldrid.VertexElementFormat.Float2 => VertexElementFormat.Float2,
            global::Veldrid.VertexElementFormat.Float3 => VertexElementFormat.Float3,
            global::Veldrid.VertexElementFormat.Float4 => VertexElementFormat.Float4,
            global::Veldrid.VertexElementFormat.UInt1 => VertexElementFormat.UInt1,
            global::Veldrid.VertexElementFormat.UInt2 => VertexElementFormat.UInt2,
            global::Veldrid.VertexElementFormat.UInt3 => VertexElementFormat.UInt3,
            global::Veldrid.VertexElementFormat.UInt4 => VertexElementFormat.UInt4,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported vertex format")
        };
    }

    internal static global::Veldrid.VertexElementFormat ConvertVertexElementFormatToVeldrid(
        VertexElementFormat vertexElementFormat)
    {
        return vertexElementFormat switch
        {
            VertexElementFormat.Float1 => global::Veldrid.VertexElementFormat.Float1,
            VertexElementFormat.Float2 => global::Veldrid.VertexElementFormat.Float2,
            VertexElementFormat.Float3 => global::Veldrid.VertexElementFormat.Float3,
            VertexElementFormat.Float4 => global::Veldrid.VertexElementFormat.Float4,
            VertexElementFormat.UInt1 => global::Veldrid.VertexElementFormat.UInt1,
            VertexElementFormat.UInt2 => global::Veldrid.VertexElementFormat.UInt2,
            VertexElementFormat.UInt3 => global::Veldrid.VertexElementFormat.UInt3,
            VertexElementFormat.UInt4 => global::Veldrid.VertexElementFormat.UInt4,
            _ => throw new ArgumentOutOfRangeException(nameof(vertexElementFormat),
                vertexElementFormat, "Unsupported vertex element format")
        };
    }

    internal static GraphicsBackend ConvertGraphicsBackendFromVeldrid(global::Veldrid.GraphicsBackend backend)
    {
        return backend switch
        {
            global::Veldrid.GraphicsBackend.Direct3D11 => GraphicsBackend.Direct3D11,
            global::Veldrid.GraphicsBackend.Vulkan => GraphicsBackend.Vulkan,
            global::Veldrid.GraphicsBackend.OpenGL => GraphicsBackend.OpenGL,
            global::Veldrid.GraphicsBackend.Metal => GraphicsBackend.Metal,
            global::Veldrid.GraphicsBackend.OpenGLES => GraphicsBackend.OpenGLES,
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unsupported graphics backend")
        };
    }

    internal static global::Veldrid.GraphicsBackend ConvertGraphicsBackendToVeldrid(GraphicsBackend backend)
    {
        return backend switch
        {
            GraphicsBackend.Direct3D11 => global::Veldrid.GraphicsBackend.Direct3D11,
            GraphicsBackend.Vulkan => global::Veldrid.GraphicsBackend.Vulkan,
            GraphicsBackend.OpenGL => global::Veldrid.GraphicsBackend.OpenGL,
            GraphicsBackend.Metal => global::Veldrid.GraphicsBackend.Metal,
            GraphicsBackend.OpenGLES => global::Veldrid.GraphicsBackend.OpenGLES,
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unsupported graphics backend")
        };
    }
}
