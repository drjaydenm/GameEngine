using GameEngine.Core.Windowing;

namespace GameEngine.Game
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new GameGame();
            var window = new SdlWindow(game);

            game.Engine.Init(window);
            game.Initialize();

            window.RunMessagePump();
        }
    }
}
