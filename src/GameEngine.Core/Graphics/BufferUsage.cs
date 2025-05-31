namespace GameEngine.Core.Graphics;

[Flags]
public enum BufferUsage
{
    /// <summary>
    /// Used for drawing commands via <see cref="ICommandList.SetVertexBuffer(uint, IBuffer)"/>.
    /// </summary>
    VertexBuffer = 1 << 0,
    /// <summary>
    /// Used for drawing commands via <see cref="ICommandList.SetIndexBuffer(IBuffer, IndexFormat)" />.
    /// </summary>
    IndexBuffer = 1 << 1,
    /// <summary>
    /// Used for a uniform buffer via a <see cref="global::Veldrid.ResourceSet"/>.
    /// </summary>
    UniformBuffer = 1 << 2,
    /// <summary>
    /// Used for a structured read-only buffer via a <see cref="global::Veldrid.ResourceSet"/>.
    /// Can only be combined with <see cref="Dynamic"/>.
    /// </summary>
    StructuredBufferReadOnly = 1 << 3,
    /// <summary>
    /// Used for a structured read-write buffer via a <see cref="global::Veldrid.ResourceSet"/>.
    /// Can not be combined with any other flag.
    /// </summary>
    StructuredBufferReadWrite = 1 << 4,
    /// <summary>
    /// Used for an indirect drawing information buffer.
    /// Can not be combined with <see cref="Dynamic"/>.
    /// </summary>
    IndirectBuffer = 1 << 5,
    /// <summary>
    /// Used for a <see cref="IBuffer"/> that will be updated with new data frequently.
    /// Can not be combined with <see cref="StructuredBufferReadWrite"/> or <see cref="IndirectBuffer"/>.
    /// </summary>
    Dynamic = 1 << 6,
    /// <summary>
    /// Used for a staging buffer to transfer data to/from the CPU using via mapping.
    /// Can not be combined with any other flag.
    /// </summary>
    Staging = 1 << 7
}
