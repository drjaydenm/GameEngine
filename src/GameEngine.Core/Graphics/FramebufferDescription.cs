namespace GameEngine.Core.Graphics;

public readonly struct FramebufferDescription : IEquatable<FramebufferDescription>
{
    public FramebufferAttachmentDescription[] ColorTargets { get; init; }
    public FramebufferAttachmentDescription? DepthTarget { get; init; }

    public FramebufferDescription(FramebufferAttachmentDescription colorTarget)
    {
        ColorTargets = [colorTarget];
    }

    public FramebufferDescription(FramebufferAttachmentDescription[] colorTargets,
        FramebufferAttachmentDescription? depthTarget)
    {
        ColorTargets = colorTargets;
        DepthTarget = depthTarget;
    }

    public bool Equals(FramebufferDescription other)
    {
        return ColorTargets.SequenceEqual(other.ColorTargets)
            && DepthTarget.HasValue == other.DepthTarget.HasValue
            && (DepthTarget?.Equals(other.DepthTarget) ?? false);
    }
}
