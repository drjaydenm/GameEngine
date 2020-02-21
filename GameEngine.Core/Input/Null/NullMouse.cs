using Veldrid;

namespace GameEngine.Core.Input.Null
{
    public class NullMouse : IMouse
    {
        public Point Position => new Point(0, 0);
        public Point PositionDelta => new Point(0, 0);
        public float ScrollDelta => 0;
        public bool IsMouseLocked
        {
            get => false;
            set => _ = value;
        }

        public bool IsButtonDown(MouseButtons button)
        {
            return false;
        }

        public bool IsButtonUp(MouseButtons button)
        {
            return true;
        }

        public bool WasButtonDown(MouseButtons button)
        {
            return false;
        }

        public bool WasButtonHeld(MouseButtons button)
        {
            return false;
        }

        public bool WasButtonPressed(MouseButtons button)
        {
            return false;
        }

        public bool WasButtonReleased(MouseButtons button)
        {
            return false;
        }

        public bool WasButtonUp(MouseButtons button)
        {
            return true;
        }
    }
}
