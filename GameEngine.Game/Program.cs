using System.IO;
using System.Reflection;
using GameEngine.Core.Windowing;

namespace GameEngine.Game
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new DemoGame();
            var window = new SdlWindow(game);
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            game.Engine.Init(window, currentDirectory);
            game.Initialize();
            game.Run();
        }
    }
}
