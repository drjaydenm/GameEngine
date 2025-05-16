namespace GameEngine.Core.Input.Null
{
    public class NullInputManager : IInputManager
    {
        public IMouse Mouse => mouse;
        public IKeyboard Keyboard => keyboard;

        private NullMouse mouse;
        private NullKeyboard keyboard;

        public NullInputManager()
        {
            mouse = new NullMouse();
            keyboard = new NullKeyboard();
        }

        public void Update()
        {
        }
    }
}
