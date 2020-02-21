namespace GameEngine.Core.Input.Null
{
    public class NullKeyboard : IKeyboard
    {
        public bool IsKeyDown(Keys key)
        {
            return false;
        }

        public bool IsKeyUp(Keys key)
        {
            return true;
        }

        public bool WasKeyDown(Keys key)
        {
            return false;
        }

        public bool WasKeyHeld(Keys key)
        {
            return false;
        }

        public bool WasKeyPressed(Keys key)
        {
            return false;
        }

        public bool WasKeyReleased(Keys key)
        {
            return false;
        }

        public bool WasKeyUp(Keys key)
        {
            return true;
        }
    }
}
