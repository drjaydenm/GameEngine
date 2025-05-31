using Veldrid;

namespace GameEngine.Core.Graphics.Veldrid;

internal class VeldridBuffer(DeviceBuffer buffer) : IBuffer
{
    public uint SizeInBytes => buffer.SizeInBytes;

    internal DeviceBuffer UnderlyingBuffer => buffer;

    public void Dispose()
    {
        buffer.Dispose();
        GC.SuppressFinalize(this);
    }
}
