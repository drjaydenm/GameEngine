using GameEngine.Core.World;

namespace GameEngine.Core.Entities
{
    public class PhysicsChunkComponent : PhysicsComponent
    {
        public Chunk Chunk { get; }

        public PhysicsChunkComponent(Chunk chunk, PhysicsInteractivity interactivity) : base(interactivity)
        {
            Chunk = chunk;
        }
    }
}
