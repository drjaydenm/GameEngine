using System;
using System.Diagnostics;

namespace GameEngine.Core
{
    public abstract class Game
    {
        public Engine Engine { get; private set; }

        private Stopwatch frameTimeStopwatch;
        private TimeSpan accumulatedTime;
        private TimeSpan lastTime;

        public Game()
        {
            Engine = new Engine();

            frameTimeStopwatch = Stopwatch.StartNew();
        }

        public virtual void Initialize()
        {
        }

        public void Run()
        {
            Engine.Window.RunMessagePump();
        }

        protected virtual void Update()
        {
            Engine.Update();
        }

        protected virtual void Draw()
        {
            Engine.Draw();
        }

        public virtual void Exit()
        {
            Engine.Window.Exit();
        }

        internal void Tick()
        {
            var currentTime = frameTimeStopwatch.Elapsed;
            var elapsedTime = currentTime - lastTime;
            lastTime = currentTime;

            if (elapsedTime > Engine.GameTimeMaxElapsed)
            {
                elapsedTime = Engine.GameTimeMaxElapsed;
            }
            Engine.GameTimeTotal += elapsedTime;
            accumulatedTime += elapsedTime;

            while (accumulatedTime >= Engine.GameTimeTargetElapsed)
            {
                Engine.GameTimeElapsed = Engine.GameTimeTargetElapsed;
                Update();
                accumulatedTime -= Engine.GameTimeTargetElapsed;
            }

            Draw();
        }
    }
}
