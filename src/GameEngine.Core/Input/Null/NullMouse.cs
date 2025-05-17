namespace GameEngine.Core.Input.Null
{
    public class NullMouse : IMouse
    {
        public Coord2 Position => Coord2.Zero;
        public Coord2 PositionDelta => Coord2.Zero;
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
