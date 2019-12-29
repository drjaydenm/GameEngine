namespace GameEngine.Core
{
    public class PerformanceCounters
    {
        public float FramesPerSecond { get; private set; }
        public float UpdatesPerSecond { get; private set; }

        private readonly Engine engine;
        private double frameTime;
        private double updateTime;
        private int frameCounter;
        private int updateCounter;

        public PerformanceCounters(Engine engine)
        {
            this.engine = engine;

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
            updateTime += engine.GameTimeElapsed.TotalMilliseconds;
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
            frameTime += engine.GameTimeElapsed.TotalMilliseconds;
        }
    }
}
