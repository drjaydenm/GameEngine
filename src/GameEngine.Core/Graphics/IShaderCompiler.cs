namespace GameEngine.Core.Graphics;

public interface IShaderCompiler
{
    Shader CompileShader(string vertexShaderCode, string fragmentShaderCode, ShaderConfig config);
}
