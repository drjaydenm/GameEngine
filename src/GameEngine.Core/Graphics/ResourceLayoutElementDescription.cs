namespace GameEngine.Core.Graphics;

public readonly struct ResourceLayoutElementDescription : IEquatable<ResourceLayoutElementDescription>
{
    public string Name { get; }
    public ResourceType Type { get; }
    public ShaderStage Stages { get; }

    public ResourceLayoutElementDescription(string name, ResourceType type, ShaderStage stages)
    {
        Name = name;
        Type = type;
        Stages = stages;
    }

    public bool Equals(ResourceLayoutElementDescription other)
    {
        return Name == other.Name && Type == other.Type && Stages == other.Stages;
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceLayoutElementDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, (int)Type, (int)Stages);
    }

    public static bool operator ==(ResourceLayoutElementDescription left, ResourceLayoutElementDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceLayoutElementDescription left, ResourceLayoutElementDescription right)
    {
        return !(left == right);
    }
}
