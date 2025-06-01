namespace GameEngine.Core.Graphics;

[Flags]
public enum ColorWriteMask
{
    None,
    Red = 1 << 0,
    Green = 1 << 1,
    Blue = 1 << 2,
    Alpha = 1 << 3,
    All = Red | Green | Blue | Alpha
}
