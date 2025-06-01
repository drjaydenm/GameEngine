namespace GameEngine.Core.Graphics;

public readonly struct StencilBehaviorDescription : IEquatable<StencilBehaviorDescription>
{
    public StencilOperation Fail { get; }
    public StencilOperation Pass { get; }
    public StencilOperation DepthFail { get; }
    public ComparisonType Comparison { get; }

    public StencilBehaviorDescription(StencilOperation fail, StencilOperation pass,
        StencilOperation depthFail, ComparisonType comparison)
    {
        Fail = fail;
        Pass = pass;
        DepthFail = depthFail;
        Comparison = comparison;
    }

    public bool Equals(StencilBehaviorDescription other)
    {
        return Fail == other.Fail
            && Pass == other.Pass
            && DepthFail == other.DepthFail
            && Comparison == other.Comparison;
    }

    public override bool Equals(object obj)
    {
        return obj is StencilBehaviorDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Fail, (int)Pass, (int)DepthFail, (int)Comparison);
    }

    public static bool operator ==(StencilBehaviorDescription left, StencilBehaviorDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StencilBehaviorDescription left, StencilBehaviorDescription right)
    {
        return !(left == right);
    }
}
