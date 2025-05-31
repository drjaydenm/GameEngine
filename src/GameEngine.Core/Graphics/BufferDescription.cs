namespace GameEngine.Core.Graphics;

public readonly struct BufferDescription : IEquatable<BufferDescription>
{
    public uint SizeInBytes { get; }
    public BufferUsage Usage { get; }
    public uint StructureByteStride { get; }
    public bool IsRawBuffer { get; }

    public BufferDescription(uint sizeInBytes, BufferUsage usage)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
        StructureByteStride = 0;
        IsRawBuffer = false;
    }

    public BufferDescription(uint sizeInBytes, BufferUsage usage, uint structureByteStride, bool isRawBuffer)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
        StructureByteStride = structureByteStride;
        IsRawBuffer = isRawBuffer;
    }

    public bool Equals(BufferDescription other)
    {
        return SizeInBytes == other.SizeInBytes
            && Usage == other.Usage
            && StructureByteStride == other.StructureByteStride
            && IsRawBuffer == other.IsRawBuffer;
    }

    public override bool Equals(object obj)
    {
        return obj is BufferDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SizeInBytes, (int)Usage, StructureByteStride, IsRawBuffer);
    }

    public static bool operator ==(BufferDescription left, BufferDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BufferDescription left, BufferDescription right)
    {
        return !(left == right);
    }
}
