namespace GameEngine.Core.Graphics;

public readonly struct ShaderSpecializationConstant : IEquatable<ShaderSpecializationConstant>
{
    public uint Id { get; }
    public ShaderConstantType Type { get; }
    public ulong Data { get; }

    public ShaderSpecializationConstant(uint id, ShaderConstantType type, ulong data)
    {
        Id = id;
        Type = type;
        Data = data;
    }

    public bool Equals(ShaderSpecializationConstant other)
    {
        return Id == other.Id && Type == other.Type && Data == other.Data;
    }

    public override bool Equals(object obj)
    {
        return obj is ShaderSpecializationConstant other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, (int)Type, Data);
    }

    public static bool operator ==(ShaderSpecializationConstant left, ShaderSpecializationConstant right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderSpecializationConstant left, ShaderSpecializationConstant right)
    {
        return !(left == right);
    }
}
