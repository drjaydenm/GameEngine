using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using GameEngine.Core.Input;

namespace GameEngine.Core.Windowing
{
    public class SdlWindow : IWindow
    {
        public event EventHandler Closing;
        public event EventHandler Resized;

        public string Title
        {
            get => window.Title;
            set => window.Title = value;
        }
        public WindowState WindowState {
            get => window.WindowState; 
            set => window.WindowState = value; 
        }
        public Vector2 Size => new Vector2(window.Width, window.Height);
        public float AspectRatio => (float)window.Width / window.Height;

        internal InputSnapshot InputSnapshot { get; private set; }
        internal Vector2 MousePosition { get; private set; }

        private Sdl2Window window;
        private Game game;

        public SdlWindow(Game game)
        {
            this.game = game;

            window = CreateWindow();
            window.Closing += () =>
            {
                Closing?.Invoke(null, EventArgs.Empty);
            };
            window.Resized += () =>
            {
                Resized?.Invoke(null, EventArgs.Empty);
            };
        }

        public void RunMessagePump()
        {
            while (window.Exists)
            {
                InputSnapshot = window.PumpEvents();
                if (game.Engine.InputManager.Mouse.IsMouseLocked && window.Focused)
                {
                    var centerScreen = new Vector2(window.Width / 2, window.Height / 2);
                    window.CursorVisible = false;
                    window.SetMousePosition(centerScreen);
                    MousePosition += InputSnapshot.MousePosition - centerScreen;
                } else
                {
                    window.CursorVisible = true;
                    MousePosition = InputSnapshot.MousePosition;
                }

                game.Update();
                game.Draw();
            }
        }

        public void Exit()
        {
            window.Close();
        }

        public GraphicsDevice CreateGraphicsDevice()
        {
            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
                );
            return VeldridStartup.CreateGraphicsDevice(window, options, GraphicsBackend.OpenGL);
        }

        public IInputManager CreateInputManager()
        {
            return new VeldridInputManager(this);
        }

        private Sdl2Window CreateWindow()
        {
            var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown;

            return new Sdl2Window(
                title: "Window",
                x: 100,
                y: 100,
                width: 960,
                height: 540,
                flags: flags,
                threadedProcessing: true);
        }
    }
}
