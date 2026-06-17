namespace GameEngine.Core.Graphics;

[Flags]
public enum TextureUsage
{
    /// <summary>
    /// Can be used as the target of a read-only texture view and can be accessed from a shader.
    /// </summary>
    Sampled = 1 << 0,
    /// <summary>
    /// Can be used as the target of a read-write texture view and can be accessed from a shader.
    /// </summary>
    Storage = 1 << 1,
    /// <summary>
    /// Can be used as the color target of a <see cref="IFramebuffer"/>.
    /// </summary>
    RenderTarget = 1 << 2,
    /// <summary>
    /// Can be used as the depth target of a <see cref="IFramebuffer"/>.
    /// </summary>
    DepthStencil = 1 << 3,
    /// <summary>
    /// The texture is a two-dimensional cubemap.
    /// </summary>
    Cubemap = 1 << 4,
    /// <summary>
    /// The texture is used as a read-write staging resource for uploading texture data.
    /// Can be mapped for writing from the CPU.
    /// </summary>
    Staging = 1 << 5,
    /// <summary>
    /// Supports generation of mipmaps.
    /// </summary>
    GenerateMipmaps = 1 << 6
}
