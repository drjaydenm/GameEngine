namespace GameEngine.Core.Graphics;

public readonly struct OutputAttachmentDescription : IEquatable<OutputAttachmentDescription>
{
    public PixelFormat Format { get; }

    public OutputAttachmentDescription(PixelFormat format)
    {
        Format = format;
    }

    public bool Equals(OutputAttachmentDescription other)
    {
        return Format == other.Format;
    }

    public override bool Equals(object obj)
    {
        return obj is OutputAttachmentDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)Format;
    }

    public static bool operator ==(OutputAttachmentDescription left, OutputAttachmentDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OutputAttachmentDescription left, OutputAttachmentDescription right)
    {
        return !(left == right);
    }
}
