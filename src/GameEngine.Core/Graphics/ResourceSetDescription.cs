namespace GameEngine.Core.Graphics;

public readonly struct ResourceSetDescription : IEquatable<ResourceSetDescription>
{
    public IResourceLayout Layout { get; }
    public IBindableResource[] Resources { get; }

    public ResourceSetDescription(IResourceLayout layout, IBindableResource[] resources)
    {
        Layout = layout;
        Resources = resources;
    }

    public bool Equals(ResourceSetDescription other)
    {
        return Equals(Layout, other.Layout) && Equals(Resources, other.Resources);
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceSetDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Layout, Resources);
    }

    public static bool operator ==(ResourceSetDescription left, ResourceSetDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceSetDescription left, ResourceSetDescription right)
    {
        return !(left == right);
    }
}
