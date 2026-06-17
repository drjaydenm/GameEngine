using System.Text;
using GameEngine.Core.Graphics;
using Veldrid;
using Veldrid.SPIRV;
using GraphicsBackend = GameEngine.Core.Graphics.GraphicsBackend;
using ResourceLayoutDescription = GameEngine.Core.Graphics.ResourceLayoutDescription;
using ResourceLayoutElementDescription = GameEngine.Core.Graphics.ResourceLayoutElementDescription;
using Shader = GameEngine.Core.Graphics.Shader;
using ShaderDescription = GameEngine.Core.Graphics.ShaderDescription;
using VertexElementDescription = GameEngine.Core.Graphics.VertexElementDescription;
using VertexElementFormat = GameEngine.Core.Graphics.VertexElementFormat;

namespace GameEngine.Graphics.ShaderCompiler;

public class VeldridSpirvShaderCompiler : IShaderCompiler
{
    private readonly IGraphicsResourceFactory _factory;
    private readonly GraphicsBackend _backend;

    public VeldridSpirvShaderCompiler(IGraphicsDevice graphicsDevice)
    {
        _factory = graphicsDevice.ResourceFactory;
        _backend = graphicsDevice.BackendType;
    }

    public Shader CompileShader(string vertexShaderCode, string fragmentShaderCode, ShaderConfig config)
    {
        var vertexShaderBytes = Encoding.UTF8.GetBytes(vertexShaderCode);
        var fragmentShaderBytes = Encoding.UTF8.GetBytes(fragmentShaderCode);
        var entryPoint = _backend == GraphicsBackend.Metal ? "main0" : "main";
        VertexElementDescription[] vertexElements;
        ResourceLayoutDescription[] resourceLayouts;

        if (_backend == GraphicsBackend.Vulkan)
        {
            vertexShaderBytes = CheckIfSpirvAndCompileIfNot(vertexShaderBytes, ShaderStage.Vertex);
            fragmentShaderBytes = CheckIfSpirvAndCompileIfNot(fragmentShaderBytes, ShaderStage.Fragment);
            vertexElements = [];
            resourceLayouts = [];
        }
        else
        {
            var options = new CrossCompileOptions();
            var target = GetCompilationTarget(_backend);

            var compilationResult = SpirvCompilation.CompileVertexFragment(
                vertexShaderBytes,
                fragmentShaderBytes,
                target,
                options
            );

            vertexShaderBytes = GetBytes(_backend, compilationResult.VertexShader);
            fragmentShaderBytes = GetBytes(_backend, compilationResult.FragmentShader);

            vertexElements = new VertexElementDescription[compilationResult.Reflection.VertexElements.Length];
            for (var i = 0; i < compilationResult.Reflection.VertexElements.Length; i++)
            {
                vertexElements[i] = new VertexElementDescription
                {
                    Name = compilationResult.Reflection.VertexElements[i].Name,
                    Format = ConvertVertexFormatFromVeldrid(compilationResult.Reflection.VertexElements[i].Format),
                    Offset = compilationResult.Reflection.VertexElements[i].Offset
                };
            }

            resourceLayouts = new ResourceLayoutDescription[compilationResult.Reflection.ResourceLayouts.Length];
            for (var i = 0; i < compilationResult.Reflection.ResourceLayouts.Length; i++)
            {
                var layout = compilationResult.Reflection.ResourceLayouts[i];
                var elements = new ResourceLayoutElementDescription[layout.Elements.Length];

                for (var j = 0; j < layout.Elements.Length; j++)
                {
                    elements[j] = new ResourceLayoutElementDescription
                    {
                        Name = layout.Elements[j].Name,
                        Type = ConvertResourceTypeFromVeldrid(layout.Elements[j].Kind),
                        Stages = (ShaderStage)layout.Elements[j].Stages
                    };
                }

                resourceLayouts[i] = new ResourceLayoutDescription { Elements = elements };
            }
        }

        var vertexShader = _factory.CreateShader(new ShaderDescription
        {
            Stage = ShaderStage.Vertex,
            ShaderBytes = vertexShaderBytes,
            EntryPoint = entryPoint,
            Debug = false
        });
        var fragmentShader = _factory.CreateShader(new ShaderDescription
        {
            Stage = ShaderStage.Fragment,
            ShaderBytes = fragmentShaderBytes,
            EntryPoint = entryPoint,
            Debug = false
        });

        return new Shader([vertexShader, fragmentShader], vertexElements, resourceLayouts, config);
    }

    private static unsafe byte[] CheckIfSpirvAndCompileIfNot(byte[] shaderBytes, ShaderStage stage)
    {
        if (HasSpirvHeader(shaderBytes))
        {
            return shaderBytes;
        }
        else
        {
            fixed (byte* sourceAsciiPtr = shaderBytes)
            {
                var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(
                    Encoding.UTF8.GetString(sourceAsciiPtr, shaderBytes.Length),
                    "shader.txt",
                    (ShaderStages)stage,
                    GlslCompileOptions.Default);

                return glslCompileResult.SpirvBytes;
            }
        }
    }

    private static bool HasSpirvHeader(byte[] bytes)
    {
        return bytes.Length > 4
           && bytes[0] == 0x03
           && bytes[1] == 0x02
           && bytes[2] == 0x23
           && bytes[3] == 0x07;
    }

    private static byte[] GetBytes(GraphicsBackend backend, string code)
    {
        switch (backend)
        {
            case GraphicsBackend.Direct3D11:
            case GraphicsBackend.OpenGL:
            case GraphicsBackend.OpenGLES:
                return Encoding.ASCII.GetBytes(code);
            case GraphicsBackend.Metal:
                return Encoding.UTF8.GetBytes(code);
            default:
                throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}");
        }
    }

    private static CrossCompileTarget GetCompilationTarget(GraphicsBackend backend)
    {
        switch (backend)
        {
            case GraphicsBackend.Direct3D11:
                return CrossCompileTarget.HLSL;
            case GraphicsBackend.OpenGL:
                return CrossCompileTarget.GLSL;
            case GraphicsBackend.Metal:
                return CrossCompileTarget.MSL;
            case GraphicsBackend.OpenGLES:
                return CrossCompileTarget.ESSL;
            default:
                throw new SpirvCompilationException($"Invalid GraphicsBackend: {backend}");
        }
    }

    private static VertexElementFormat ConvertVertexFormatFromVeldrid(global::Veldrid.VertexElementFormat format)
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

    private static ResourceType ConvertResourceTypeFromVeldrid(ResourceKind resourceKind)
    {
        return resourceKind switch
        {
            ResourceKind.UniformBuffer => ResourceType.UniformBuffer,
            ResourceKind.StructuredBufferReadOnly => ResourceType.StructuredBufferReadOnly,
            ResourceKind.StructuredBufferReadWrite => ResourceType.StructuredBufferReadWrite,
            ResourceKind.TextureReadOnly => ResourceType.TextureReadOnly,
            ResourceKind.TextureReadWrite => ResourceType.TextureReadWrite,
            ResourceKind.Sampler => ResourceType.Sampler,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceKind), resourceKind, "Unsupported resource kind")
        };
    }
}
