namespace GameEngine.Core.Graphics;

public readonly struct BlendAttachmentDescription : IEquatable<BlendAttachmentDescription>
{
    public static readonly BlendAttachmentDescription OverrideBlend = new()
    {
        BlendEnabled = true,
        ColorWriteMask = null,
        SourceColorFactor = BlendFactor.One,
        DestinationColorFactor = BlendFactor.Zero,
        ColorFunction = BlendFunction.Add,
        SourceAlphaFactor = BlendFactor.One,
        DestinationAlphaFactor = BlendFactor.Zero,
        AlphaFunction = BlendFunction.Add
    };

    public static readonly BlendAttachmentDescription AlphaBlend = new()
    {
        BlendEnabled = true,
        ColorWriteMask = null,
        SourceColorFactor = BlendFactor.SourceAlpha,
        DestinationColorFactor = BlendFactor.InverseSourceAlpha,
        ColorFunction = BlendFunction.Add,
        SourceAlphaFactor = BlendFactor.SourceAlpha,
        DestinationAlphaFactor = BlendFactor.InverseSourceAlpha,
        AlphaFunction = BlendFunction.Add
    };

    public bool BlendEnabled { get; init; }
    public ColorWriteMask? ColorWriteMask { get; init; }
    public BlendFactor SourceColorFactor { get; init; }
    public BlendFactor DestinationColorFactor { get; init; }
    public BlendFunction ColorFunction { get; init; }
    public BlendFactor SourceAlphaFactor { get; init; }
    public BlendFactor DestinationAlphaFactor { get; init; }
    public BlendFunction AlphaFunction { get; init; }

    public BlendAttachmentDescription(bool blendEnabled, ColorWriteMask? colorWriteMask, BlendFactor sourceColorFactor,
        BlendFactor destinationColorFactor, BlendFunction colorFunction, BlendFactor sourceAlphaFactor,
        BlendFactor destinationAlphaFactor, BlendFunction alphaFunction)
    {
        BlendEnabled = blendEnabled;
        ColorWriteMask = colorWriteMask;
        SourceColorFactor = sourceColorFactor;
        DestinationColorFactor = destinationColorFactor;
        ColorFunction = colorFunction;
        SourceAlphaFactor = sourceAlphaFactor;
        DestinationAlphaFactor = destinationAlphaFactor;
        AlphaFunction = alphaFunction;
    }

    public bool Equals(BlendAttachmentDescription other)
    {
        return BlendEnabled == other.BlendEnabled
            && ColorWriteMask == other.ColorWriteMask
            && SourceColorFactor == other.SourceColorFactor
            && DestinationColorFactor == other.DestinationColorFactor
            && ColorFunction == other.ColorFunction
            && SourceAlphaFactor == other.SourceAlphaFactor
            && DestinationAlphaFactor == other.DestinationAlphaFactor
            && AlphaFunction == other.AlphaFunction;
    }

    public override bool Equals(object obj)
    {
        return obj is BlendAttachmentDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlendEnabled, ColorWriteMask, (int)SourceColorFactor, (int)DestinationColorFactor,
            (int)ColorFunction, (int)SourceAlphaFactor, (int)DestinationAlphaFactor, (int)AlphaFunction);
    }

    public static bool operator ==(BlendAttachmentDescription left, BlendAttachmentDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlendAttachmentDescription left, BlendAttachmentDescription right)
    {
        return !(left == right);
    }
}
