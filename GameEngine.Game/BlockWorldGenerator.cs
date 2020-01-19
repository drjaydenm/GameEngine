using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameEngine.Core;
using GameEngine.Core.World;

namespace GameEngine.Game
{
    public class BlockWorldGenerator
    {
        private const float LARGE_NOISE_SCALE = 60;
        private const float SMALL_NOISE_SCALE = 10;

        private readonly BlockWorld world;
        private readonly Noise largeNoise;
        private readonly Noise smallNoise;

        public BlockWorldGenerator(BlockWorld world)
        {
            this.world = world;
            largeNoise = new Noise(123);
            largeNoise.SetFrequency(0.009f);
            smallNoise = new Noise(456);
            smallNoise.SetFrequency(0.05f);
        }

        public IEnumerable<Chunk> GenerateWorld(int xSize, int ySize, int zSize)
        {
            var chunks = new ConcurrentBag<Chunk>();
            var startXOffset = xSize / 2;
            var startYOffset = ySize / 2;
            var startZOffset = zSize / 2;

            Parallel.For(-startXOffset, xSize - startXOffset, (x) =>
            {
                Parallel.For(-startYOffset, ySize - startYOffset, (y) =>
                {
                    Parallel.For(-startZOffset, zSize - startZOffset, (z) =>
                    {
                        chunks.Add(GenerateChunk(x, y, z));
                    });
                });
            });

            return chunks;
        }

        public Chunk GenerateChunk(int xPos, int yPos, int zPos)
        {
            var chunk = world.CreateChunk(new Coord3(xPos, yPos, zPos));
            GenerateChunk(chunk);

            return chunk;
        }

        public void GenerateChunk(Chunk chunk)
        {
            for (var x = 0; x < Chunk.CHUNK_X_SIZE; x++)
            {
                for (var z = 0; z < Chunk.CHUNK_Z_SIZE; z++)
                {
                    var chunkXBlockStart = chunk.Coordinate.X * Chunk.CHUNK_X_SIZE;
                    var chunkYBlockStart = chunk.Coordinate.Y * Chunk.CHUNK_Y_SIZE;
                    var chunkZBlockStart = chunk.Coordinate.Z * Chunk.CHUNK_Z_SIZE;
                    var noiseValue1 = GetNoise(largeNoise, x + chunkXBlockStart, z + chunkZBlockStart);
                    var noiseValue2 = GetNoise(smallNoise, x + chunkXBlockStart, z + chunkZBlockStart);

                    var height = noiseValue1 * LARGE_NOISE_SCALE;
                    height += noiseValue2 * SMALL_NOISE_SCALE;

                    for (var y = 0; y < Chunk.CHUNK_Y_SIZE; y++)
                    {
                        var actualYBlockStart = y + chunkYBlockStart;
                        var blockType = BlockType.Dirt;

                        if (actualYBlockStart <= 10)
                        {
                            blockType = BlockType.Grass;
                        }
                        if (actualYBlockStart > 40)
                        {
                            blockType = BlockType.Stone;
                        }

                        var isEnabled = y + chunkYBlockStart <= height;

                        chunk.SetBlockIsActive(x, y, z, isEnabled);
                        chunk.SetBlockType(x, y, z, (byte)blockType);
                    }
                }
            }
        }

        private float GetNoise(Noise noise, int x, int y)
        {
            return Math.Clamp((0.63f + noise.GetPerlin(x, y)) / 1.2f, 0, 1);
        }
    }
}
