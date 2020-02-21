using System;
using System.Numerics;
using Veldrid;
using GameEngine.Core.Input;
using GameEngine.Core.Content;

namespace GameEngine.Core.Windowing
{
    public interface IWindow
    {
        event EventHandler Closing;
        event EventHandler Resized;

        string Title { get; set; }
        Vector2 Position { get; }
        Vector2 Size { get; set; }
        Vector2 ScreenSize { get; }
        float AspectRatio { get; }
        WindowState WindowState { get; set; }
        bool Running { get; }

        GraphicsDevice CreateGraphicsDevice();
        IInputManager CreateInputManager();
        IContentLoader CreateContentLoader();
        void PumpMessages();
        void Exit();
    }
}
