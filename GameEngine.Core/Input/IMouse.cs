using Veldrid;

namespace GameEngine.Core.Input
{
    public interface IMouse
    {
        bool IsButtonDown(MouseButtons button);
        bool IsButtonUp(MouseButtons button);

        bool WasButtonDown(MouseButtons button);
        bool WasButtonUp(MouseButtons button);

        bool WasButtonHeld(MouseButtons button);
        bool WasButtonPressed(MouseButtons button);
        bool WasButtonReleased(MouseButtons button);

        Point Position { get; }
        Point PositionDelta { get; }
        float ScrollDelta { get; }
        bool IsMouseLocked { get; set; }
    }
}
