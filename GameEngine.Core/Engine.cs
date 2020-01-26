using System;
using System.Collections.Generic;
using Veldrid;
using GameEngine.Core.Graphics;
using GameEngine.Core.Input;
using GameEngine.Core.Windowing;
using GameEngine.Core.Threading;

namespace GameEngine.Core
{
    public class Engine
    {
        public TimeSpan GameTimeTotal { get; internal set; }
        public TimeSpan GameTimeElapsed { get; internal set; }
        public TimeSpan GameTimeTargetElapsed => TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        public TimeSpan GameTimeMaxElapsed => TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 10);
        public PerformanceCounters PerformanceCounters { get; }
        public IWindow Window { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public IInputManager InputManager { get; private set; }
        public DebugGraphics DebugGraphics { get; private set; }
        public TextRenderer TextRenderer { get; private set; }
        public JobQueues Jobs { get; private set; }

        internal CommandList CommandList { get; private set; }

        private bool isShuttingDown;
        private List<Scene> scenes;
        private List<Scene> scenesToRemove;
        private List<Scene> scenesToAdd;
        private bool scenesDirty;

        public Engine()
        {
            PerformanceCounters = new PerformanceCounters(this);

            scenes = new List<Scene>();
            scenesToRemove = new List<Scene>();
            scenesToAdd = new List<Scene>();
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
            GameTimeElapsed = TimeSpan.Zero;

            GraphicsDevice = Window.CreateGraphicsDevice();
            InputManager = Window.CreateInputManager();

            CommandList = GraphicsDevice.ResourceFactory.CreateCommandList();

            DebugGraphics = new DebugGraphics(this);
            TextRenderer = new TextRenderer(this);
            Jobs = new JobQueues();
        }

        public void Update()
        {
            if (isShuttingDown)
                return;

            PerformanceCounters.Update();

            InputManager.Update();

            TextRenderer.Update();

            foreach (var scene in scenes)
            {
                scene.Update();
            }

            if (scenesDirty)
            {
                foreach (var scene in scenesToRemove)
                {
                    scenes.Remove(scene);
                }
                foreach (var scene in scenesToAdd)
                {
                    scenes.Add(scene);
                }
                scenesToRemove.Clear();
                scenesToAdd.Clear();
                scenesDirty = false;
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

            TextRenderer.Draw();

            CommandList.End();
            GraphicsDevice.SubmitCommands(CommandList);
            GraphicsDevice.SwapBuffers();
        }

        public void Shutdown()
        {
            Jobs.ShutdownQueues();
        }

        public void AddScene(Scene scene)
        {
            scenesToAdd.Add(scene);
            scenesDirty = true;
        }

        public void RemoveScene(Scene scene)
        {
            scenesToRemove.Add(scene);
            scenesDirty = true;
        }

        internal void WaitForVsync()
        {
            GraphicsDevice.WaitForIdle();
        }

        private void OnWindowResized()
        {
            GraphicsDevice.ResizeMainWindow((uint)Window.Size.X, (uint)Window.Size.Y);
        }
    }
}
