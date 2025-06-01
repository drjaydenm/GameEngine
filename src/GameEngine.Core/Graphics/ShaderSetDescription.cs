namespace GameEngine.Core.Graphics;

public readonly struct ShaderSetDescription : IEquatable<ShaderSetDescription>
{
    public VertexLayoutDescription[] VertexLayouts { get; init; }
    public Shader Shader { get; init; }
    public ShaderSpecializationConstant[] Specializations { get; init; }

    public ShaderSetDescription(VertexLayoutDescription[] vertexLayouts, Shader shader,
        ShaderSpecializationConstant[] specializations)
    {
        VertexLayouts = vertexLayouts;
        Shader = shader;
        Specializations = specializations;
    }

    public bool Equals(ShaderSetDescription other)
    {
        return Equals(VertexLayouts, other.VertexLayouts)
            && Equals(Shader, other.Shader)
            && Equals(Specializations, other.Specializations);
    }

    public override bool Equals(object obj)
    {
        return obj is ShaderSetDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(VertexLayouts, Shader, Specializations);
    }

    public static bool operator ==(ShaderSetDescription left, ShaderSetDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderSetDescription left, ShaderSetDescription right)
    {
        return !(left == right);
    }
}
