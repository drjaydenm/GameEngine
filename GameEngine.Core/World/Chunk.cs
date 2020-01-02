using System.Numerics;

namespace GameEngine.Core.World
{
    public struct Chunk
    {
        public const int CHUNK_X_SIZE = 16;
        public const int CHUNK_Y_SIZE = 16;
        public const int CHUNK_Z_SIZE = 16;
        public static readonly Vector3 CHUNK_SIZE = new Vector3(CHUNK_X_SIZE, CHUNK_Y_SIZE, CHUNK_Z_SIZE);
        public static readonly Vector3 CHUNK_BLOCK_RATIO = new Vector3(1f / CHUNK_X_SIZE, 1f / CHUNK_Y_SIZE, 1f / CHUNK_Z_SIZE);

        public Block[,,] Blocks { get; private set; }
        public Coord3 Coordinate { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Vector3 WorldPositionCentroid { get; private set; }

        public Chunk(Coord3 coordinate)
        {
            Coordinate = coordinate;
            WorldPosition = coordinate * CHUNK_SIZE;
            WorldPositionCentroid = WorldPosition + (CHUNK_SIZE * 0.5f);

            Blocks = new Block[CHUNK_X_SIZE, CHUNK_Y_SIZE, CHUNK_Z_SIZE];
        }

        public bool IsAnyBlockActive()
        {
            return IsAnyBlock(true);
        }

        public bool IsAnyBlockInactive()
        {
            return IsAnyBlock(false);
        }

        private bool IsAnyBlock(bool active)
        {
            for (var x = 0; x < Blocks.GetLength(0); x++)
            {
                for (var y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (var z = 0; z < Blocks.GetLength(2); z++)
                    {
                        if (Blocks[x, y, z].IsActive == active) return true;
                    }
                }
            }

            return false;
        }
    }
}
