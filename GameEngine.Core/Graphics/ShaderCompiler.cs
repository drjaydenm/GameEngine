using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace GameEngine.Core.Graphics
{
    /// <summary>
    /// Compiles shaders and preserves reflection data for resources within the shaders
    /// Taken from https://github.com/mellinoe/veldrid-spirv/blob/master/src/Veldrid.SPIRV/ResourceFactoryExtensions.cs
    /// </summary>
    public static class ShaderCompiler
    {
        public static Shader CompileShader(Engine engine, string vertexShaderCode, string fragmentShaderCode)
        {
            var vertexShaderDescription = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(vertexShaderCode),
                "main");
            var fragmentShaderDescription = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(fragmentShaderCode),
                "main");

            var factory = engine.GraphicsDevice.ResourceFactory;

            var backend = factory.BackendType;
            if (backend == GraphicsBackend.Vulkan)
            {
                vertexShaderDescription.ShaderBytes = EnsureSpirv(vertexShaderDescription);
                fragmentShaderDescription.ShaderBytes = EnsureSpirv(fragmentShaderDescription);

                return new Shader(new[]
                {
                    factory.CreateShader(ref vertexShaderDescription),
                    factory.CreateShader(ref fragmentShaderDescription)
                }, null);
            }

            var options = new CrossCompileOptions();
            var target = GetCompilationTarget(factory.BackendType);
            var compilationResult = SpirvCompilation.CompileVertexFragment(
                vertexShaderDescription.ShaderBytes,
                fragmentShaderDescription.ShaderBytes,
                target,
                options);

            var vertexEntryPoint = (backend == GraphicsBackend.Metal && vertexShaderDescription.EntryPoint == "main")
                ? "main0"
                : vertexShaderDescription.EntryPoint;
            var vertexBytes = GetBytes(backend, compilationResult.VertexShader);
            var vertexShader = factory.CreateShader(new ShaderDescription(
                vertexShaderDescription.Stage,
                vertexBytes,
                vertexEntryPoint));

            var fragmentEntryPoint = (backend == GraphicsBackend.Metal && fragmentShaderDescription.EntryPoint == "main")
                ? "main0"
                : fragmentShaderDescription.EntryPoint;
            var fragmentBytes = GetBytes(backend, compilationResult.FragmentShader);
            var fragmentShader = factory.CreateShader(new ShaderDescription(
                fragmentShaderDescription.Stage,
                fragmentBytes,
                fragmentEntryPoint));

            return new Shader(new[] { vertexShader, fragmentShader }, compilationResult.Reflection);
        }

        private static unsafe byte[] EnsureSpirv(ShaderDescription description)
        {
            if (HasSpirvHeader(description.ShaderBytes))
            {
                return description.ShaderBytes;
            }
            else
            {
                fixed (byte* sourceAsciiPtr = description.ShaderBytes)
                {
                    var glslCompileResult = SpirvCompilation.CompileGlslToSpirv(
                        Encoding.UTF8.GetString(sourceAsciiPtr, description.ShaderBytes.Length),
                        "shader.txt",
                        description.Stage,
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
    }
}
