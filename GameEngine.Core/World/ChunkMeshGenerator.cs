using System.Collections.Generic;
using System.Numerics;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.World
{
    public class ChunkMeshGenerator
    {
        public static Mesh<VertexPositionNormalMaterial> GenerateMesh(Chunk chunk, BlockWorld world)
        {
            var vertices = new List<VertexPositionNormalMaterial>();
            var indicies = new List<uint>();

            // Find these once off as they are the same for the whole chunk
            var chunkToTop = world.FindChunkByOffset(chunk, Coord3.UnitY);
            var chunkToBottom = world.FindChunkByOffset(chunk, -Coord3.UnitY);
            var chunkToLeft = world.FindChunkByOffset(chunk, -Coord3.UnitX);
            var chunkToRight = world.FindChunkByOffset(chunk, Coord3.UnitX);
            var chunkToBack = world.FindChunkByOffset(chunk, -Coord3.UnitZ);
            var chunkToFront = world.FindChunkByOffset(chunk, Coord3.UnitZ);

            for (int blockX = 0; blockX < chunk.Blocks.GetLength(0); blockX++)
            {
                for (int blockY = 0; blockY < chunk.Blocks.GetLength(1); blockY++)
                {
                    for (int blockZ = 0; blockZ < chunk.Blocks.GetLength(2); blockZ++)
                    {
                        if (!chunk.Blocks[blockX, blockY, blockZ].IsActive)
                            continue;

                        var blockOffset = new Vector3(blockX, blockY, blockZ);
                        var blockType = chunk.Blocks[blockX, blockY, blockZ].BlockType;

                        // Top
                        if ((blockY == Chunk.CHUNK_Y_SIZE - 1 && !(chunkToTop?.Blocks[blockX, 0, blockZ].IsActive ?? true))
                            || (blockY < Chunk.CHUNK_Y_SIZE - 1 && !chunk.Blocks[blockX, blockY + 1, blockZ].IsActive))
                        {
                            var startIndex = (uint)vertices.Count;
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitY, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitY, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitY, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitY, blockType));

                            indicies.AddRange(CreateFaceIndicies(startIndex));
                        }
                        // Bottom
                        if ((blockY == 0 && !(chunkToBottom?.Blocks[blockX, Chunk.CHUNK_Y_SIZE - 1, blockZ].IsActive ?? true))
                            || (blockY > 0 && !chunk.Blocks[blockX, blockY - 1, blockZ].IsActive))
                        {
                            var startIndex = (uint)vertices.Count;
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitY, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitY, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitY, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitY, blockType));

                            indicies.AddRange(CreateFaceIndicies(startIndex));
                        }
                        // Left
                        if ((blockX == 0 && !(chunkToLeft?.Blocks[Chunk.CHUNK_Y_SIZE - 1, blockY, blockZ].IsActive ?? true))
                            || (blockX > 0 && !chunk.Blocks[blockX - 1, blockY, blockZ].IsActive))
                        {
                            var startIndex = (uint)vertices.Count;
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitX, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, -Vector3.UnitX, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitX, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitX, blockType));

                            indicies.AddRange(CreateFaceIndicies(startIndex));
                        }
                        // Right
                        if ((blockX == Chunk.CHUNK_X_SIZE - 1 && !(chunkToRight?.Blocks[0, blockY, blockZ].IsActive ?? true))
                            || (blockX < Chunk.CHUNK_X_SIZE - 1 && !chunk.Blocks[blockX + 1, blockY, blockZ].IsActive))
                        {
                            var startIndex = (uint)vertices.Count;
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitX, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitX, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, Vector3.UnitX, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitX, blockType));

                            indicies.AddRange(CreateFaceIndicies(startIndex));
                        }
                        // Back
                        if ((blockZ == 0 && !(chunkToBack?.Blocks[blockX, blockY, Chunk.CHUNK_Z_SIZE - 1].IsActive ?? true))
                            || (blockZ > 0 && !chunk.Blocks[blockX, blockY, blockZ - 1].IsActive))
                        {
                            var startIndex = (uint)vertices.Count;
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType));

                            indicies.AddRange(CreateFaceIndicies(startIndex));
                        }
                        // Front
                        if ((blockZ == Chunk.CHUNK_Z_SIZE - 1 && !(chunkToFront?.Blocks[blockX, blockY, 0].IsActive ?? true))
                            || (blockZ < Chunk.CHUNK_Z_SIZE - 1 && !chunk.Blocks[blockX, blockY, blockZ + 1].IsActive))
                        {
                            var startIndex = (uint)vertices.Count;
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType));
                            vertices.Add(new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType));

                            indicies.AddRange(CreateFaceIndicies(startIndex));
                        }
                    }
                }
            }

            if (vertices.Count <= 0)
                return null;

            return new Mesh<VertexPositionNormalMaterial>(vertices.ToArray(), indicies.ToArray());
        }

        private static uint[] CreateFaceIndicies(uint vertexStartOffset)
        {
            return new uint[]
            {
                0 + vertexStartOffset,
                1 + vertexStartOffset,
                2 + vertexStartOffset,
                0 + vertexStartOffset,
                2 + vertexStartOffset,
                3 + vertexStartOffset
            };
        }
    }
}
