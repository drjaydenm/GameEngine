using System;
using System.Collections.Generic;
using Veldrid;
using GameEngine.Core.Graphics;
using GameEngine.Core.Input;
using GameEngine.Core.Windowing;

namespace GameEngine.Core
{
    public class Engine
    {
        public TimeSpan GameTimeTotal { get; private set; }
        public TimeSpan GameTimeElapsed { get; private set; }
        public PerformanceCounters PerformanceCounters { get; }
        public IWindow Window { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public IInputManager InputManager { get; private set; }
        public DebugGraphics DebugGraphics { get; private set; }

        internal CommandList CommandList { get; private set; }

        private DateTime lastUpdated;
        private bool isShuttingDown;
        private List<Scene> scenes;

        public Engine()
        {
            PerformanceCounters = new PerformanceCounters(this);

            scenes = new List<Scene>();
        }

        public void Init(IWindow window)
        {
            Window = window;
            Window.Resized += (sender, e) =>
            {
                OnWindowResized();
            };
            Window.Closing += (sender, e) =>
            {
                isShuttingDown = true;
            };

            GameTimeTotal = TimeSpan.Zero;
            GameTimeTotal = TimeSpan.Zero;
            lastUpdated = DateTime.Now;

            GraphicsDevice = Window.CreateGraphicsDevice();
            InputManager = Window.CreateInputManager();

            CommandList = GraphicsDevice.ResourceFactory.CreateCommandList();

            DebugGraphics = new DebugGraphics(this);
        }

        public void Update()
        {
            if (isShuttingDown)
                return;

            GameTimeElapsed = DateTime.Now - lastUpdated;
            GameTimeTotal += GameTimeElapsed;
            lastUpdated = DateTime.Now;

            PerformanceCounters.Update();

            InputManager.Update();

            foreach (var scene in scenes)
            {
                scene.Update();
            }
        }

        public void Draw()
        {
            if (isShuttingDown)
                return;

            PerformanceCounters.Draw();

            CommandList.Begin();
            CommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);

            CommandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            CommandList.ClearDepthStencil(1f);

            foreach (var scene in scenes)
            {
                scene.Draw();
            }

            CommandList.End();
            GraphicsDevice.SubmitCommands(CommandList);

            GraphicsDevice.WaitForIdle();
            GraphicsDevice.SwapBuffers();
        }

        public void AddScene(Scene scene)
        {
            scenes.Add(scene);
        }

        public void RemoveScene(Scene scene)
        {
            scenes.Remove(scene);
        }

        private void OnWindowResized()
        {
            GraphicsDevice.ResizeMainWindow((uint)Window.Size.X, (uint)Window.Size.Y);
        }
    }
}
