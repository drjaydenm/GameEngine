namespace GameEngine.Core.Graphics;

public readonly struct ResourceLayoutDescription : IEquatable<ResourceLayoutDescription>
{
    public ResourceLayoutElementDescription[] Elements { get; }

    public ResourceLayoutDescription(params ResourceLayoutElementDescription[] elements)
    {
        Elements = elements;
    }

    public bool Equals(ResourceLayoutDescription other)
    {
        return Equals(Elements, other.Elements);
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceLayoutDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Elements != null ? Elements.GetHashCode() : 0;
    }

    public static bool operator ==(ResourceLayoutDescription left, ResourceLayoutDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceLayoutDescription left, ResourceLayoutDescription right)
    {
        return !(left == right);
    }
}
