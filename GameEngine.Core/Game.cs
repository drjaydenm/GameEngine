namespace GameEngine.Core
{
    public abstract class Game
    {
        public Engine Engine { get; private set; }

        public Game()
        {
            Engine = new Engine();
        }

        public virtual void Initialize()
        {
        }

        public virtual void Update()
        {
            Engine.Update();
        }

        public virtual void Draw()
        {
            Engine.Draw();
        }

        public virtual void Exit()
        {
            Engine.Window.Exit();
        }
    }
}
