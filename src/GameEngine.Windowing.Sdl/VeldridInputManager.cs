using GameEngine.Core.Input;
using GameEngine.Core.Windowing;

namespace GameEngine.Windowing.Sdl
{
    public class VeldridInputManager : IInputManager
    {
        public IMouse Mouse => mouse;
        public IKeyboard Keyboard => keyboard;

        private readonly SdlWindow window;
        private readonly VeldridMouse mouse;
        private readonly VeldridKeyboard keyboard;

        public VeldridInputManager(SdlWindow window)
        {
            this.window = window;

            mouse = new VeldridMouse();
            keyboard = new VeldridKeyboard();
        }

        public void Update()
        {
            mouse.Update(window.InputSnapshot, window.MousePosition);
            keyboard.Update(window.InputSnapshot);
        }
    }
}
