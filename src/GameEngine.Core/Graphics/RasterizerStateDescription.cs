namespace GameEngine.Core.Graphics;

public readonly struct RasterizerStateDescription : IEquatable<RasterizerStateDescription>
{
    public FaceCullMode CullMode { get; init; }
    public PolygonFillMode FillMode { get; init; }
    public FrontFace FrontFace { get; init; }
    public bool DepthClipEnabled { get; init; }
    public bool ScissorTestEnabled { get; init; }

    public RasterizerStateDescription(FaceCullMode cullMode, PolygonFillMode fillMode,
        FrontFace frontFace, bool depthClipEnabled, bool scissorTestEnabled)
    {
        CullMode = cullMode;
        FillMode = fillMode;
        FrontFace = frontFace;
        DepthClipEnabled = depthClipEnabled;
        ScissorTestEnabled = scissorTestEnabled;
    }

    public bool Equals(RasterizerStateDescription other)
    {
        return CullMode == other.CullMode
            && FillMode == other.FillMode
            && FrontFace == other.FrontFace
            && DepthClipEnabled == other.DepthClipEnabled
            && ScissorTestEnabled == other.ScissorTestEnabled;
    }

    public override bool Equals(object obj)
    {
        return obj is RasterizerStateDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)CullMode, (int)FillMode, (int)FrontFace, DepthClipEnabled, ScissorTestEnabled);
    }

    public static bool operator ==(RasterizerStateDescription left, RasterizerStateDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RasterizerStateDescription left, RasterizerStateDescription right)
    {
        return !(left == right);
    }
}
