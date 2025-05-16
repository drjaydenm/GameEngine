using GameEngine.Core.Windowing;

namespace GameEngine.Core.Input.Veldrid
{
    public class VeldridInputManager : IInputManager
    {
        public IMouse Mouse => mouse;
        public IKeyboard Keyboard => keyboard;

        private readonly SdlWindow sdlWindow;
        private readonly VeldridMouse mouse;
        private readonly VeldridKeyboard keyboard;

        public VeldridInputManager(SdlWindow sdlWindow)
        {
            this.sdlWindow = sdlWindow;

            mouse = new VeldridMouse();
            keyboard = new VeldridKeyboard();
        }

        public void Update()
        {
            mouse.Update(sdlWindow.InputSnapshot, sdlWindow.MousePosition);
            keyboard.Update(sdlWindow.InputSnapshot);
        }
    }
}
