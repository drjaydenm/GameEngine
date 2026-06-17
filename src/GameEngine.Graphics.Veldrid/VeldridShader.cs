using GameEngine.Core.Graphics;
using Shader = Veldrid.Shader;

namespace GameEngine.Graphics.Veldrid;

public class VeldridShader(Shader shader) : IShader
{
    internal Shader UnderlyingShader => shader;

    public void Dispose()
    {
        shader.Dispose();
        GC.SuppressFinalize(this);
    }
}
