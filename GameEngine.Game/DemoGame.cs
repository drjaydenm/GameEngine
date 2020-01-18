using GameEngine.Core;
using GameEngine.Core.Graphics;

namespace GameEngine.Game
{
    public class DemoGame : Core.Game
    {
        public LoadingScene LoadingScene { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            Engine.Window.Title = "DemoGame";
            Engine.Window.Size = Engine.Window.ScreenSize - (Engine.Window.Position * 2);

            LoadingScene = new LoadingScene();
            LoadingScene.Initialize(Engine, new Renderer(Engine, LoadingScene), null);
            LoadingScene.LoadScene();

            Engine.AddScene(LoadingScene);
        }
    }
}
