namespace GameEngine.Core.Input
{
    public interface IInputManager
    {
        IMouse Mouse { get; }
        IKeyboard Keyboard { get; }

        void Update();
    }
}
