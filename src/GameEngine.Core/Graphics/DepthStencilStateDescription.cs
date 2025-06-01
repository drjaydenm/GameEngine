namespace GameEngine.Core.Graphics;

public readonly struct DepthStencilStateDescription : IEquatable<DepthStencilStateDescription>
{
    public bool DepthTestEnabled { get; init; }
    public bool DepthWriteEnabled { get; init; }
    public ComparisonType ComparisonType { get; init; }
    public bool StencilTestEnabled { get; init; }
    public StencilBehaviorDescription StencilFront { get; init; }
    public StencilBehaviorDescription StencilBack { get; init; }
    public byte StencilReadMask { get; init; }
    public byte StencilWriteMask { get; init; }
    public uint StencilReference { get; init; }

    public DepthStencilStateDescription(bool depthTestEnabled, bool depthWriteEnabled, ComparisonType depthComparison,
        bool stencilTestEnabled, StencilBehaviorDescription stencilFront, StencilBehaviorDescription stencilBack,
        byte stencilReadMask, byte stencilWriteMask, uint stencilReference)
    {
        DepthTestEnabled = depthTestEnabled;
        DepthWriteEnabled = depthWriteEnabled;
        ComparisonType = depthComparison;
        StencilTestEnabled = stencilTestEnabled;
        StencilFront = stencilFront;
        StencilBack = stencilBack;
        StencilReadMask = stencilReadMask;
        StencilWriteMask = stencilWriteMask;
        StencilReference = stencilReference;
    }

    public bool Equals(DepthStencilStateDescription other)
    {
        return DepthTestEnabled == other.DepthTestEnabled
            && DepthWriteEnabled == other.DepthWriteEnabled
            && ComparisonType == other.ComparisonType
            && StencilTestEnabled == other.StencilTestEnabled
            && StencilFront.Equals(other.StencilFront)
            && StencilBack.Equals(other.StencilBack)
            && StencilReadMask == other.StencilReadMask
            && StencilWriteMask == other.StencilWriteMask
            && StencilReference == other.StencilReference;
    }

    public override bool Equals(object obj)
    {
        return obj is DepthStencilStateDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(DepthTestEnabled);
        hashCode.Add(DepthWriteEnabled);
        hashCode.Add((int)ComparisonType);
        hashCode.Add(StencilTestEnabled);
        hashCode.Add(StencilFront);
        hashCode.Add(StencilBack);
        hashCode.Add(StencilReadMask);
        hashCode.Add(StencilWriteMask);
        hashCode.Add(StencilReference);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(DepthStencilStateDescription left, DepthStencilStateDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DepthStencilStateDescription left, DepthStencilStateDescription right)
    {
        return !(left == right);
    }
}
