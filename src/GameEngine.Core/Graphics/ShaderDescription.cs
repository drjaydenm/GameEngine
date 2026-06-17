namespace GameEngine.Core.Graphics;

public readonly struct ShaderDescription : IEquatable<ShaderDescription>
{
    public ShaderStage Stage { get; init; }
    public byte[] ShaderBytes { get; init; }
    public string EntryPoint { get; init; }
    public bool Debug { get; init; }

    public ShaderDescription(ShaderStage stage, byte[] shaderBytes, string entryPoint, bool debug)
    {
        Stage = stage;
        ShaderBytes = shaderBytes;
        EntryPoint = entryPoint;
        Debug = debug;
    }

    public bool Equals(ShaderDescription other)
    {
        return Stage == other.Stage
            && Equals(ShaderBytes, other.ShaderBytes)
            && EntryPoint == other.EntryPoint
            && Debug == other.Debug;
    }

    public override bool Equals(object obj)
    {
        return obj is ShaderDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Stage, ShaderBytes, EntryPoint, Debug);
    }

    public static bool operator ==(ShaderDescription left, ShaderDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShaderDescription left, ShaderDescription right)
    {
        return !(left == right);
    }
}
