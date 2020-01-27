using System;
using System.Buffers;
using System.Numerics;

namespace GameEngine.Core.World
{
    public class Chunk : IDisposable
    {
        public const int CHUNK_X_SIZE = 16;
        public const int CHUNK_Y_SIZE = 16;
        public const int CHUNK_Z_SIZE = 16;
        public const int CHUNK_BLOCK_COUNT = CHUNK_X_SIZE * CHUNK_Y_SIZE * CHUNK_Z_SIZE;
        public static readonly Vector3 CHUNK_SIZE = new Vector3(CHUNK_X_SIZE, CHUNK_Y_SIZE, CHUNK_Z_SIZE);
        public static readonly Vector3 CHUNK_BLOCK_RATIO = new Vector3(1f / CHUNK_X_SIZE, 1f / CHUNK_Y_SIZE, 1f / CHUNK_Z_SIZE);

        public Block[] Blocks => blocks;
        public Coord3 Coordinate { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public Vector3 WorldPositionCentroid { get; private set; }

        public int ActiveBlockCount => CHUNK_BLOCK_COUNT - InactiveBlockCount;
        public int InactiveBlockCount => inactiveBlockCount;
        public bool IsAnyBlockActive => InactiveBlockCount < CHUNK_BLOCK_COUNT;
        public bool IsAnyBlockInactive => InactiveBlockCount > 0;

        private Block[] blocks;
        private int inactiveBlockCount;
        private ArrayPool<Block> blockPool;

        public Chunk(Coord3 coordinate, ArrayPool<Block> blockPool)
        {
            this.blockPool = blockPool;
            Coordinate = coordinate;
            WorldPosition = coordinate * CHUNK_SIZE;
            WorldPositionCentroid = WorldPosition + (CHUNK_SIZE * 0.5f);

            blocks = blockPool.Rent(CHUNK_X_SIZE * CHUNK_Y_SIZE * CHUNK_Z_SIZE);
            for (var i = 0; i < blocks.Length; i++)
            {
                blocks[i] = default;
            }

            inactiveBlockCount = CHUNK_BLOCK_COUNT;
        }

        public void SetBlockIsActive(int x, int y, int z, bool isActive)
        {
            ref var block = ref blocks[x + (y * CHUNK_X_SIZE) + (z * CHUNK_X_SIZE * CHUNK_Y_SIZE)];

            if (block.IsActive && !isActive)
                inactiveBlockCount++;
            else if (!block.IsActive && isActive)
                inactiveBlockCount--;

            block.IsActive = isActive;
        }

        public void SetBlockType(int x, int y, int z, byte blockType)
        {
            blocks[x + (y * CHUNK_X_SIZE) + (z * CHUNK_X_SIZE * CHUNK_Y_SIZE)].BlockType = blockType;
        }

        public void Dispose()
        {
            blockPool.Return(blocks);
            blocks = null;
        }
    }
}
