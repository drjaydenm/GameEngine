﻿using System.Numerics;
using Veldrid;
using GameEngine.Core.Input;
using GameEngine.Core.Content;
using GameEngine.Core.Audio;

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
        bool IsDriven { get; }

        GraphicsDevice CreateGraphicsDevice();
        IInputManager CreateInputManager();
        IContentLoader CreateContentLoader();
        IAudioSystem CreateAudioSystem();
        void PumpMessages();
        void StartDrivenLoop(Action tick);
        void Exit();
    }
}
