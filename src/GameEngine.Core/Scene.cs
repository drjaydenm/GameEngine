using GameEngine.Core.Camera;
using GameEngine.Core.Entities;
using GameEngine.Core.Graphics;
using GameEngine.Core.Physics;

namespace GameEngine.Core
{
    public class Scene
    {
        public IReadOnlyList<Entity> Entities => entities;
        public Renderer Renderer { get; private set; }
        public IPhysicsSystem PhysicsSystem { get; private set; }
        public ICamera ActiveCamera { get; private set; }
        public Engine Engine { get; private set; }
        public Game Game { get; private set; }

        private readonly List<Entity> entities;

        public Scene()
        {
            entities = new List<Entity>();
        }

        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);
        }

        public void Initialize(Engine engine, Game game, Renderer renderer, IPhysicsSystem physicsSystem)
        {
            Engine = engine;
            Game = game;
            Renderer = renderer;
            PhysicsSystem = physicsSystem;
        }

        public virtual void LoadScene()
        {
        }

        public virtual void Update()
        {
            if (PhysicsSystem != null)
                PhysicsSystem.Update();

            for (var i = 0; i < entities.Count; i++)
            {
                entities[i].Update();
            }
        }

        public virtual void Draw()
        {
            Renderer.Draw();

            if (PhysicsSystem != null)
                PhysicsSystem.Draw();
        }

        public void SetActiveCamera(ICamera camera)
        {
            ActiveCamera = camera;
        }
    }
}
