using Veldrid;

namespace GameEngine.Core.Input
{
    public interface IKeyboard
    {
        bool IsKeyDown(Keys key);
        bool IsKeyUp(Keys key);

        bool WasKeyDown(Keys key);
        bool WasKeyUp(Keys key);

        bool WasKeyHeld(Keys key);
        bool WasKeyPressed(Keys key);
        bool WasKeyReleased(Keys key);
    }
}
