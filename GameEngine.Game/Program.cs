using GameEngine.Core.Windowing;

namespace GameEngine.Game
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new DemoGame();
            var window = new SdlWindow(game);

            game.Engine.Init(window);
            game.Initialize();
            game.Run();
        }
    }
}
