namespace GameEngine.Core.Graphics;

public readonly struct OutputDescription : IEquatable<OutputDescription>
{
    public OutputAttachmentDescription? DepthAttachment { get; init; }
    public OutputAttachmentDescription[] ColorAttachments { get; init; }
    public TextureSampleCount SampleCount { get; init; }

    public OutputDescription(OutputAttachmentDescription? depthAttachment,
        OutputAttachmentDescription[] colorAttachments, TextureSampleCount sampleCount)
    {
        DepthAttachment = depthAttachment;
        ColorAttachments = colorAttachments;
        SampleCount = sampleCount;
    }

    public bool Equals(OutputDescription other)
    {
        return Nullable.Equals(DepthAttachment, other.DepthAttachment)
            && Equals(ColorAttachments, other.ColorAttachments)
            && SampleCount == other.SampleCount;
    }

    public override bool Equals(object obj)
    {
        return obj is OutputDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DepthAttachment, ColorAttachments, (int)SampleCount);
    }

    public static bool operator ==(OutputDescription left, OutputDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OutputDescription left, OutputDescription right)
    {
        return !(left == right);
    }
}
