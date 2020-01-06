using System.Numerics;

namespace GameEngine.Core.World
{
    public class Chunk
    {
        public const int CHUNK_X_SIZE = 16;
        public const int CHUNK_Y_SIZE = 16;
        public const int CHUNK_Z_SIZE = 16;
        public const int CHUNK_BLOCK_COUNT = CHUNK_X_SIZE * CHUNK_Y_SIZE * CHUNK_Z_SIZE;
        public static readonly Vector3 CHUNK_SIZE = new Vector3(CHUNK_X_SIZE, CHUNK_Y_SIZE, CHUNK_Z_SIZE);
        public static readonly Vector3 CHUNK_BLOCK_RATIO = new Vector3(1f / CHUNK_X_SIZE, 1f / CHUNK_Y_SIZE, 1f / CHUNK_Z_SIZE);

        public Block[,,] Blocks { get; private set; }
        public Coord3 Coordinate { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Vector3 WorldPositionCentroid { get; private set; }

        public int ActiveBlockCount => CHUNK_BLOCK_COUNT - InactiveBlockCount;
        public int InactiveBlockCount { get; private set; }
        public bool IsAnyBlockActive => InactiveBlockCount < CHUNK_BLOCK_COUNT;
        public bool IsAnyBlockInactive => InactiveBlockCount > 0;

        public Chunk(Coord3 coordinate)
        {
            Coordinate = coordinate;
            WorldPosition = coordinate * CHUNK_SIZE;
            WorldPositionCentroid = WorldPosition + (CHUNK_SIZE * 0.5f);

            Blocks = new Block[CHUNK_X_SIZE, CHUNK_Y_SIZE, CHUNK_Z_SIZE];
            InactiveBlockCount = CHUNK_BLOCK_COUNT;
        }

        public void SetBlockIsActive(int x, int y, int z, bool isActive)
        {
            ref var block = ref Blocks[x, y, z];

            var wasActive = block.IsActive;
            block.IsActive = isActive;

            if (wasActive && !isActive)
                InactiveBlockCount++;
            else if (!wasActive && isActive)
                InactiveBlockCount--;
        }

        public void SetBlockType(int x, int y, int z, byte blockType)
        {
            Blocks[x, y, z].BlockType = blockType;
        }
    }
}
