using System;
using System.Diagnostics;

namespace GameEngine.Core
{
    public class PerformanceCounters
    {
        public float FramesPerSecond { get; private set; }
        public float UpdatesPerSecond { get; private set; }

        private readonly Engine engine;
        private Stopwatch stopwatch;
        private TimeSpan lastUpdate;
        private TimeSpan lastFrame;
        private double frameTime;
        private double updateTime;
        private int frameCounter;
        private int updateCounter;

        public PerformanceCounters(Engine engine)
        {
            this.engine = engine;
            stopwatch = Stopwatch.StartNew();

            FramesPerSecond = 0;
            UpdatesPerSecond = 0;
        }

        public void Update()
        {
            if (updateTime >= 1000)
            {
                UpdatesPerSecond = updateCounter;
                updateCounter = 0;
                updateTime = 0;
            }

            updateCounter++;

            var currentTime = engine.GameTimeTotal;
            updateTime += (currentTime - lastUpdate).TotalMilliseconds;
            lastUpdate = currentTime;
        }

        public void Draw()
        {
            if (frameTime >= 1000)
            {
                FramesPerSecond = frameCounter;
                frameCounter = 0;
                frameTime = 0;
            }

            frameCounter++;

            var currentTime = stopwatch.Elapsed;
            frameTime += (currentTime - lastFrame).TotalMilliseconds;
            lastFrame = currentTime;
        }
    }
}
