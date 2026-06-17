namespace GameEngine.Core.Graphics;

public interface IFramebuffer : IDisposable
{
    OutputDescription OutputDescription { get; }
    uint Width { get; }
    uint Height { get; }
}
