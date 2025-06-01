namespace GameEngine.Core.Graphics;

public readonly struct VertexElementDescription : IEquatable<VertexElementDescription>
{
    public string Name { get; }
    public VertexElementFormat Format { get; }
    public uint Offset { get; }

    public VertexElementDescription(string name, VertexElementFormat format, uint offset)
    {
        Name = name;
        Format = format;
        Offset = offset;
    }

    public bool Equals(VertexElementDescription other)
    {
        return Name == other.Name && Format == other.Format && Offset == other.Offset;
    }

    public override bool Equals(object obj)
    {
        return obj is VertexElementDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, (int)Format, Offset);
    }

    public static bool operator ==(VertexElementDescription left, VertexElementDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexElementDescription left, VertexElementDescription right)
    {
        return !(left == right);
    }
}
