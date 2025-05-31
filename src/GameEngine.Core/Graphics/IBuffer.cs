namespace GameEngine.Core.Graphics;

public interface IBuffer : IDisposable, IBindableResource
{
    uint SizeInBytes { get; }
}
