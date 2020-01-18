using GameEngine.Core.Entities;

namespace GameEngine.Core.World
{
    public class LoadedChunk
    {
        public Chunk Chunk { get; set; }
        public ChunkRenderable Renderable { get; set; }
        public PhysicsComponent Physics { get; set; }

        public LoadedChunk(Chunk chunk)
        {
            Chunk = chunk;
            Renderable = null;
            Physics = null;
        }
    }
}
