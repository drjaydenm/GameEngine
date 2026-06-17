namespace GameEngine.Core.Graphics;

public readonly struct TextureDescription : IEquatable<TextureDescription>
{
    public uint Width { get; init; }
    public uint Height { get; init; }
    public uint Depth { get; init; }
    public uint MipLevels { get; init; }
    public uint ArrayLayers { get; init; }
    public PixelFormat Format { get; init; }
    public TextureUsage Usage { get; init; }
    public TextureType Type { get; init; }
    public TextureSampleCount SampleCount { get; init; }

    public TextureDescription(uint width, uint height, uint depth, uint mipLevels, uint arrayLayers,
        PixelFormat format, TextureUsage usage, TextureType type, TextureSampleCount sampleCount)
    {
        Width = width;
        Height = height;
        Depth = depth;
        MipLevels = mipLevels;
        ArrayLayers = arrayLayers;
        Format = format;
        Usage = usage;
        Type = type;
        SampleCount = sampleCount;
    }

    public bool Equals(TextureDescription other)
    {
        return Width == other.Width
            && Height == other.Height
            && Depth == other.Depth
            && MipLevels == other.MipLevels
            && ArrayLayers == other.ArrayLayers
            && Format == other.Format
            && Usage == other.Usage
            && Type == other.Type
            && SampleCount == other.SampleCount;
    }

    public override bool Equals(object obj)
    {
        return obj is TextureDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Width);
        hashCode.Add(Height);
        hashCode.Add(Depth);
        hashCode.Add(MipLevels);
        hashCode.Add(ArrayLayers);
        hashCode.Add((int)Format);
        hashCode.Add((int)Usage);
        hashCode.Add((int)Type);
        hashCode.Add((int)SampleCount);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(TextureDescription left, TextureDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextureDescription left, TextureDescription right)
    {
        return !(left == right);
    }
}
