namespace GameEngine.Core.Graphics;

public readonly struct VertexLayoutDescription : IEquatable<VertexLayoutDescription>
{
    public uint Stride { get; init; }
    public VertexElementDescription[] Elements { get; init; }
    public uint InstanceStepRate { get; init; }

    public VertexLayoutDescription(uint stride, VertexElementDescription[] elements, uint instanceStepRate)
    {
        Stride = stride;
        Elements = elements;
        InstanceStepRate = instanceStepRate;
    }

    public bool Equals(VertexLayoutDescription other)
    {
        return Stride == other.Stride && Equals(Elements, other.Elements) && InstanceStepRate == other.InstanceStepRate;
    }

    public override bool Equals(object obj)
    {
        return obj is VertexLayoutDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Stride, Elements, InstanceStepRate);
    }

    public static bool operator ==(VertexLayoutDescription left, VertexLayoutDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexLayoutDescription left, VertexLayoutDescription right)
    {
        return !(left == right);
    }
}
