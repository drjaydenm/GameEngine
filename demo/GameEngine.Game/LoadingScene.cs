using System.Numerics;
using GameEngine.Core;
using GameEngine.Core.Camera;
using GameEngine.Core.Graphics;
using GameEngine.Core.Input;
using GameEngine.Core.Physics.BepuPhysics;

namespace GameEngine.Game
{
    public class LoadingScene : Scene
    {
        public GameScene GameScene { get; private set; }

        public override void LoadScene()
        {
            base.LoadScene();

            SetActiveCamera(new StaticCamera(Vector3.Zero, Vector3.UnitZ, 1f, 0.1f, 10f, Engine.Window.AspectRatio));
        }

        public override void Update()
        {
            base.Update();

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.Escape))
            {
                Game.Exit();
            }

            var font = "Content/Fonts/OpenSans-Regular.woff";
            var fontColor = Color.White;
            Engine.TextRenderer.DrawText($"FPS: {Engine.PerformanceCounters.FramesPerSecond} / UPS: {Engine.PerformanceCounters.UpdatesPerSecond}", new Vector2(5, 5), fontColor, font, 15);

            var loadingTextPos = (Engine.Window.Size / 2f) - new Vector2(150, 25);
            Engine.TextRenderer.DrawText("Loading...", loadingTextPos, fontColor, font, 50);

            if (Engine.GameTimeTotal.TotalMilliseconds > 500)
            {
                LoadGameScene();
            }
        }

        public void LoadGameScene()
        {
            GameScene = new GameScene();
            GameScene.Initialize(Engine, Game, new Renderer(Engine, GameScene), new BepuPhysicsSystem(Engine, GameScene, new Vector3(0, -9.8f, 0)));
            GameScene.LoadScene();

            Engine.AddScene(GameScene);
            Engine.RemoveScene(this);
        }
    }
}
