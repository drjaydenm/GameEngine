using GameEngine.Core.World;

namespace GameEngine.Core.Entities
{
    public class PhysicsChunkComponent : PhysicsComponent
    {
        public Chunk Chunk { get; }

        public PhysicsChunkComponent(Chunk chunk) : base(PhysicsInteractivity.Static)
        {
            Chunk = chunk;
        }
    }
}
