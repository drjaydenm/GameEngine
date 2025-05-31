namespace GameEngine.Core.Graphics;

[Flags]
public enum ShaderStage
{
    None = 0,
    Vertex = 1 << 0,
    Geometry = 1 << 1,
    TessellationControl = 1 << 2,
    TessellationEvaluation = 1 << 3,
    Fragment = 1 << 4,
    Compute = 1 << 5
}
