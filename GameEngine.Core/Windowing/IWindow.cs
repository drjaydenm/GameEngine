using System;
using System.Numerics;
using Veldrid;
using GameEngine.Core.Input;

namespace GameEngine.Core.Windowing
{
    public interface IWindow
    {
        event EventHandler Closing;
        event EventHandler Resized;

        string Title { get; set; }
        Vector2 Size { get; }
        float AspectRatio { get; }
        WindowState WindowState { get; set; }
        bool Running { get; }

        GraphicsDevice CreateGraphicsDevice();
        IInputManager CreateInputManager();
        void PumpMessages();
        void Exit();
    }
}
