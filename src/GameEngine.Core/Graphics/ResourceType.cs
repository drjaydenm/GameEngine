namespace GameEngine.Core.Graphics;

public enum ResourceType : byte
{
    UniformBuffer,
    StructuredBufferReadOnly,
    StructuredBufferReadWrite,
    TextureReadOnly,
    TextureReadWrite,
    Sampler
}
