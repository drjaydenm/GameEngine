using System;
using System.Collections.Generic;
using GameEngine.Core;
using GameEngine.Core.World;

namespace GameEngine.Game
{
    public class BlockWorldGenerator
    {
        private const int MOUNTAIN_END_HEIGHT = 60;
        private const int GROUND_START_HEIGHT = 0;

        private readonly Random random = new Random();
        private readonly Noise noise1;
        private readonly Noise noise2;

        public BlockWorldGenerator()
        {
            noise1 = new Noise(123);
            noise2 = new Noise(456);
        }

        public IEnumerable<Chunk> GenerateWorld(int xSize, int ySize, int zSize)
        {
            var chunks = new List<Chunk>();
            var startXOffset = xSize / 2;
            var startYOffset = ySize / 2;
            var startZOffset = zSize / 2;
            
            for (var x = -startXOffset; x < xSize - startXOffset; x++)
            {
                for (var y = -startYOffset; y < ySize - startYOffset; y++)
                {
                    for (var z = -startZOffset; z < zSize - startZOffset; z++)
                    {
                        chunks.Add(GenerateChunk(x, y, z));
                    }
                }
            }

            return chunks;
        }

        public Chunk GenerateChunk(int xPos, int yPos, int zPos)
        {
            var chunk = new Chunk(new Coord3(xPos, yPos, zPos));
            for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
            {
                for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
                {
                    var chunkXBlockStart = chunk.Coordinate.X * Chunk.CHUNK_X_SIZE;
                    var chunkYBlockStart = chunk.Coordinate.Y * Chunk.CHUNK_Y_SIZE;
                    var chunkZBlockStart = chunk.Coordinate.Z * Chunk.CHUNK_Z_SIZE;
                    var noiseValue1 = GetNoise(noise1, x + chunkXBlockStart, z + chunkZBlockStart);
                    var noiseValue2 = GetNoise(noise2, x + chunkXBlockStart, z + chunkZBlockStart);

                    var height = GROUND_START_HEIGHT + (noiseValue1 * (MOUNTAIN_END_HEIGHT - GROUND_START_HEIGHT));
                    height += (noiseValue2 - 0.5f) * 20f;

                    for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
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

                        chunk.Blocks[x, y, z] = new Block(isEnabled, (uint)blockType, null);
                    }
                }
            }

            return chunk;
        }

        private float GetNoise(Noise noise, int x, int y)
        {
            return Math.Clamp((0.63f + noise.GetPerlin(x, y)) / 1.2f, 0, 1);
        }
    }
}
