using System.Numerics;
using System.Runtime.InteropServices;
using GameEngine.Core.Audio;
using GameEngine.Core.Audio.OpenAL;
using GameEngine.Core.Content;
using GameEngine.Core.Input;
using GameEngine.Core.Input.Veldrid;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace GameEngine.Core.Windowing.Sdl
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
        public WindowState WindowState
        {
            get => ConvertFromSdlWindowState(window.WindowState);
            set => window.WindowState = ConvertToSdlWindowState(value);
        }
        public Vector2 Position => new Vector2(window.X, window.Y);
        public Vector2 Size
        {
            get => new Vector2(window.Width, window.Height);
            set
            {
                window.Width = (int)value.X;
                window.Height = (int)value.Y;
            }
        }
        public unsafe Vector2 ScreenSize
        {
            get
            {
                SDL_DisplayMode displayMode;
                Sdl2Native.SDL_GetDesktopDisplayMode(0, &displayMode);

                return new Vector2(displayMode.w, displayMode.h);
            }
        }
        public float AspectRatio => (float)window.Width / window.Height;
        public bool Running => window.Exists;
        public bool IsDriven => false;

        internal InputSnapshot InputSnapshot { get; private set; }
        internal Vector2 MousePosition { get; private set; }

        private Sdl2Window window;
        private Game game;
        private GraphicsBackend backend;

        public SdlWindow(Game game, GraphicsBackend backend)
        {
            this.game = game;
            this.backend = backend;

            Init();
        }

        public SdlWindow(Game game)
        {
            this.game = game;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                backend = GraphicsBackend.Direct3D11;
            }
            else
            {
                backend = GraphicsBackend.OpenGL;
            }

            Init();
        }

        public void Exit()
        {
            window.Close();
        }

        public void PumpMessages()
        {
            InputSnapshot = window.PumpEvents();
            if (game.Engine.InputManager.Mouse.IsMouseLocked && window.Focused)
            {
                var centerScreen = new Vector2(window.Width / 2, window.Height / 2);
                window.CursorVisible = false;
                window.SetMousePosition(centerScreen);
                MousePosition += InputSnapshot.MousePosition - centerScreen;
            }
            else
            {
                window.CursorVisible = true;
                MousePosition = InputSnapshot.MousePosition;
            }
        }

        public void StartDrivenLoop(Action tick)
        {
        }

        public GraphicsDevice CreateGraphicsDevice()
        {
            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true
                );
            return VeldridStartup.CreateGraphicsDevice(window, options, backend);
        }

        public IInputManager CreateInputManager()
        {
            return new VeldridInputManager(this);
        }

        public IContentLoader CreateContentLoader()
        {
            return new FileContentLoader();
        }

        public IAudioSystem CreateAudioSystem()
        {
            return new OpenALAudioSystem();
        }

        private void Init()
        {
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

        private Sdl2Window CreateWindow()
        {
            var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown;

            var window = new Sdl2Window(
                title: "Window",
                x: 35,
                y: 35,
                width: 800,
                height: 600,
                flags: flags,
                threadedProcessing: false);

            return window;
        }

        private static WindowState ConvertFromSdlWindowState(Veldrid.WindowState state)
        {
            return state switch
            {
                Veldrid.WindowState.Normal => WindowState.Normal,
                Veldrid.WindowState.FullScreen => WindowState.FullScreen,
                Veldrid.WindowState.Maximized => WindowState.Maximized,
                Veldrid.WindowState.Minimized => WindowState.Minimized,
                Veldrid.WindowState.BorderlessFullScreen => WindowState.BorderlessFullScreen,
                Veldrid.WindowState.Hidden => WindowState.Hidden,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unsupported window state")
            };
        }

        private static Veldrid.WindowState ConvertToSdlWindowState(WindowState state)
        {
            return state switch
            {
                WindowState.Normal => Veldrid.WindowState.Normal,
                WindowState.FullScreen => Veldrid.WindowState.FullScreen,
                WindowState.Maximized => Veldrid.WindowState.Maximized,
                WindowState.Minimized => Veldrid.WindowState.Minimized,
                WindowState.BorderlessFullScreen => Veldrid.WindowState.BorderlessFullScreen,
                WindowState.Hidden => Veldrid.WindowState.Hidden,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unsupported window state")
            };
        }
    }
}
