namespace GameEngine.Core.Graphics;

public readonly struct BlendStateDescription : IEquatable<BlendStateDescription>
{
    public static readonly BlendStateDescription SingleOverrideBlend = new()
    {
        AttachmentStates = [BlendAttachmentDescription.OverrideBlend]
    };

    public static readonly BlendStateDescription SingleAlphaBlend = new()
    {
        AttachmentStates = [BlendAttachmentDescription.AlphaBlend]
    };

    public Color BlendFactor { get; init; }
    public BlendAttachmentDescription[] AttachmentStates { get; init; }
    public bool AlphaToCoverageEnabled { get; init; }

    public BlendStateDescription(Color blendFactor, BlendAttachmentDescription[] attachmentStates,
        bool alphaToCoverageEnabled)
    {
        BlendFactor = blendFactor;
        AttachmentStates = attachmentStates;
        AlphaToCoverageEnabled = alphaToCoverageEnabled;
    }

    public bool Equals(BlendStateDescription other)
    {
        return BlendFactor.Equals(other.BlendFactor)
            && Equals(AttachmentStates, other.AttachmentStates)
            && AlphaToCoverageEnabled == other.AlphaToCoverageEnabled;
    }

    public override bool Equals(object obj)
    {
        return obj is BlendStateDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlendFactor, AttachmentStates, AlphaToCoverageEnabled);
    }

    public static bool operator ==(BlendStateDescription left, BlendStateDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlendStateDescription left, BlendStateDescription right)
    {
        return !(left == right);
    }
}
