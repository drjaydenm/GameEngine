using System.Numerics;
using GameEngine.Core;
using GameEngine.Core.Graphics;
using GameEngine.Core.Physics.BepuPhysics;

namespace GameEngine.Game
{
    public class DemoGame : Core.Game
    {
        public GameScene GameScene { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            GameScene = new GameScene(Engine);
            GameScene.Initialize(new Renderer(Engine, GameScene), new BepuPhysicsSystem(Engine, GameScene, new Vector3(0, -9.8f, 0)));
            GameScene.LoadScene();

            Engine.AddScene(GameScene);
        }

        public override void Update()
        {
            base.Update();

            Engine.Window.Title = $"DemoGame FPS: {Engine.PerformanceCounters.FramesPerSecond}" +
                $" UPS: {Engine.PerformanceCounters.UpdatesPerSecond}" +
                $" Chunks: {GameScene.ChunkCount}" +
                $" Chunks Queued: {GameScene.ChunkQueuedCount}";
        }
    }
}
