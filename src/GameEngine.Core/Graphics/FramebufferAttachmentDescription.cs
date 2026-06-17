namespace GameEngine.Core.Graphics;

public readonly struct FramebufferAttachmentDescription : IEquatable<FramebufferAttachmentDescription>
{
    public ITexture Target { get; init; }
    public uint ArrayLayer { get; init; }
    public uint MipLevel { get; init; }

    public FramebufferAttachmentDescription(ITexture target, uint arrayLayer = 0, uint mipLevel = 0)
    {
        Target = target;
        ArrayLayer = arrayLayer;
        MipLevel = mipLevel;
    }

    public bool Equals(FramebufferAttachmentDescription other)
    {
        return  Target == other.Target && ArrayLayer == other.ArrayLayer && MipLevel == other.MipLevel;
    }
}
