using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.World
{
    public class ChunkMeshGenerator
    {
        public static Mesh<VertexPositionNormalMaterial> GenerateMesh(Chunk chunk, BlockWorld world)
        {
            // Give these lists some initial capacity to save the inevitable resizing
            var vertices = new VertexPositionNormalMaterial[3000];
            var indices = new uint[4500];
            uint vertexCount = 0;
            uint indexCount = 0;

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
                        ref var block = ref chunk.Blocks[blockX, blockY, blockZ];
                        if (!block.IsActive)
                            continue;

                        var blockOffset = new Vector3(blockX, blockY, blockZ);
                        var blockType = block.BlockType;

                        // Top
                        if ((blockY == Chunk.CHUNK_Y_SIZE - 1 && !(chunkToTop?.Blocks[blockX, 0, blockZ].IsActive ?? true))
                            || (blockY < Chunk.CHUNK_Y_SIZE - 1 && !chunk.Blocks[blockX, blockY + 1, blockZ].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                Array.Resize(ref vertices, vertices.Length * 2);

                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitY, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitY, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitY, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Bottom
                        if ((blockY == 0 && !(chunkToBottom?.Blocks[blockX, Chunk.CHUNK_Y_SIZE - 1, blockZ].IsActive ?? true))
                            || (blockY > 0 && !chunk.Blocks[blockX, blockY - 1, blockZ].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                Array.Resize(ref vertices, vertices.Length * 2);

                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitY, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitY, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitY, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Left
                        if ((blockX == 0 && !(chunkToLeft?.Blocks[Chunk.CHUNK_Y_SIZE - 1, blockY, blockZ].IsActive ?? true))
                            || (blockX > 0 && !chunk.Blocks[blockX - 1, blockY, blockZ].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                Array.Resize(ref vertices, vertices.Length * 2);

                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, -Vector3.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitX, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Right
                        if ((blockX == Chunk.CHUNK_X_SIZE - 1 && !(chunkToRight?.Blocks[0, blockY, blockZ].IsActive ?? true))
                            || (blockX < Chunk.CHUNK_X_SIZE - 1 && !chunk.Blocks[blockX + 1, blockY, blockZ].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                Array.Resize(ref vertices, vertices.Length * 2);

                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, Vector3.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitX, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Back
                        if ((blockZ == 0 && !(chunkToBack?.Blocks[blockX, blockY, Chunk.CHUNK_Z_SIZE - 1].IsActive ?? true))
                            || (blockZ > 0 && !chunk.Blocks[blockX, blockY, blockZ - 1].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                Array.Resize(ref vertices, vertices.Length * 2);

                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Front
                        if ((blockZ == Chunk.CHUNK_Z_SIZE - 1 && !(chunkToFront?.Blocks[blockX, blockY, 0].IsActive ?? true))
                            || (blockZ < Chunk.CHUNK_Z_SIZE - 1 && !chunk.Blocks[blockX, blockY, blockZ + 1].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                Array.Resize(ref vertices, vertices.Length * 2);

                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitZ, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                    }
                }
            }

            if (vertexCount <= 0)
                return null;

            return new Mesh<VertexPositionNormalMaterial>(ref vertices, ref indices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddFaceIndices(ref uint[] indices, ref uint indexCount, uint vertexStartOffset)
        {
            if (indexCount + 6 > indices.Length)
                Array.Resize(ref indices, indices.Length * 2);

            indices[indexCount++] = 0 + vertexStartOffset;
            indices[indexCount++] = 1 + vertexStartOffset;
            indices[indexCount++] = 2 + vertexStartOffset;
            indices[indexCount++] = 0 + vertexStartOffset;
            indices[indexCount++] = 2 + vertexStartOffset;
            indices[indexCount++] = 3 + vertexStartOffset;
        }
    }
}
