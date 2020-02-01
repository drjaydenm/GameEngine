using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.World
{
    public class ChunkMeshGenerator
    {
        private ArrayPool<VertexPositionNormalTexCoordMaterial> vertexPool;
        private ArrayPool<uint> indexPool;

        public ChunkMeshGenerator()
        {
            var numBlocksPerChunk = Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE * Chunk.CHUNK_Z_SIZE;
            vertexPool = ArrayPool<VertexPositionNormalTexCoordMaterial>.Create(4 * 6 * numBlocksPerChunk, 10);
            indexPool = ArrayPool<uint>.Create(6 * 6 * numBlocksPerChunk, 10);
        }

        public void GenerateMesh(Chunk chunk, BlockWorld world, out VertexPositionNormalTexCoordMaterial[] outVertices, out uint vertexCount, out uint[] outIndices, out uint indexCount)
        {
            vertexCount = 0;
            indexCount = 0;

            // Find these once off as they are the same for the whole chunk
            var chunkToTop = world.FindChunkByOffset(chunk, Coord3.UnitY);
            var chunkToBottom = world.FindChunkByOffset(chunk, -Coord3.UnitY);
            var chunkToLeft = world.FindChunkByOffset(chunk, -Coord3.UnitX);
            var chunkToRight = world.FindChunkByOffset(chunk, Coord3.UnitX);
            var chunkToBack = world.FindChunkByOffset(chunk, -Coord3.UnitZ);
            var chunkToFront = world.FindChunkByOffset(chunk, Coord3.UnitZ);

            // Do a quick little check to see if this chunk is easily concealed and can be hidden
            if ((chunkToTop == null || !chunkToTop.IsAnyBlockInactive)
                && (chunkToBottom == null || !chunkToBottom.IsAnyBlockInactive)
                && (chunkToLeft == null || !chunkToLeft.IsAnyBlockInactive)
                && (chunkToRight == null || !chunkToRight.IsAnyBlockInactive)
                && (chunkToBack == null || !chunkToBack.IsAnyBlockInactive)
                && (chunkToFront == null || !chunkToFront.IsAnyBlockInactive))
            {
                outVertices = null;
                outIndices = null;
                return;
            }

            // Give these lists some initial capacity to save the inevitable resizing
            var vertices = vertexPool.Rent(3000);
            var indices = indexPool.Rent(4500);

            for (int blockX = 0; blockX < Chunk.CHUNK_X_SIZE; blockX++)
            {
                for (int blockY = 0; blockY < Chunk.CHUNK_Y_SIZE; blockY++)
                {
                    for (int blockZ = 0; blockZ < Chunk.CHUNK_Z_SIZE; blockZ++)
                    {
                        ref var block = ref chunk.Blocks[blockX + (blockY * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)];
                        if (!block.IsActive)
                            continue;

                        var blockOffset = new Vector3(blockX, blockY, blockZ);
                        var blockType = block.BlockType;

                        // Top
                        if ((blockY == Chunk.CHUNK_Y_SIZE - 1 && !(chunkToTop?.Blocks[blockX + (0 * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive ?? true))
                            || (blockY < Chunk.CHUNK_Y_SIZE - 1 && !chunk.Blocks[blockX + ((blockY + 1) * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                ResizeVertexArray(ref vertices);

                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitY, Vector2.Zero, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitY, Vector2.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitY, Vector2.One, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitY, Vector2.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Bottom
                        if ((blockY == 0 && !(chunkToBottom?.Blocks[blockX + ((Chunk.CHUNK_Y_SIZE - 1) * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive ?? true))
                            || (blockY > 0 && !chunk.Blocks[blockX + ((blockY - 1) * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                ResizeVertexArray(ref vertices);

                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitY, Vector2.Zero, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitY, Vector2.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitY, Vector2.One, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitY, Vector2.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Left
                        if ((blockX == 0 && !(chunkToLeft?.Blocks[(Chunk.CHUNK_X_SIZE - 1) + (blockY * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive ?? true))
                            || (blockX > 0 && !chunk.Blocks[blockX - 1 + (blockY * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                ResizeVertexArray(ref vertices);

                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitX, Vector2.Zero, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, -Vector3.UnitX, Vector2.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, -Vector3.UnitX, Vector2.One, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitX, Vector2.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Right
                        if ((blockX == Chunk.CHUNK_X_SIZE - 1 && !(chunkToRight?.Blocks[0 + (blockY * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive ?? true))
                            || (blockX < Chunk.CHUNK_X_SIZE - 1 && !chunk.Blocks[blockX + 1 + (blockY * Chunk.CHUNK_X_SIZE) + (blockZ * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                ResizeVertexArray(ref vertices);

                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitX, Vector2.Zero, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, Vector3.UnitX, Vector2.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, Vector3.UnitX, Vector2.One, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitX, Vector2.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Back
                        if ((blockZ == 0 && !(chunkToBack?.Blocks[blockX + (blockY * Chunk.CHUNK_X_SIZE) + ((Chunk.CHUNK_Z_SIZE - 1) * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive ?? true))
                            || (blockZ > 0 && !chunk.Blocks[blockX + (blockY * Chunk.CHUNK_X_SIZE) + ((blockZ - 1) * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                ResizeVertexArray(ref vertices);

                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, Vector2.Zero, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, +0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, Vector2.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, Vector2.One, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, -0.5f, -0.5f) + blockOffset, -Vector3.UnitZ, Vector2.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                        // Front
                        if ((blockZ == Chunk.CHUNK_Z_SIZE - 1 && !(chunkToFront?.Blocks[blockX + (blockY * Chunk.CHUNK_X_SIZE) + (0 * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive ?? true))
                            || (blockZ < Chunk.CHUNK_Z_SIZE - 1 && !chunk.Blocks[blockX + (blockY * Chunk.CHUNK_X_SIZE) + ((blockZ + 1) * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive))
                        {
                            if (vertexCount + 4 > vertices.Length)
                                ResizeVertexArray(ref vertices);

                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitZ, Vector2.Zero, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, +0.5f, +0.5f) + blockOffset, Vector3.UnitZ, Vector2.UnitX, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(+0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitZ, Vector2.One, blockType);
                            vertices[vertexCount++] = new VertexPositionNormalTexCoordMaterial(new Vector3(-0.5f, -0.5f, +0.5f) + blockOffset, Vector3.UnitZ, Vector2.UnitY, blockType);

                            AddFaceIndices(ref indices, ref indexCount, vertexCount - 4);
                        }
                    }
                }
            }

            if (vertexCount <= 0)
            {
                outVertices = null;
                outIndices = null;
                FreeBuffers(vertices, indices);
                return;
            }

            outVertices = vertices;
            outIndices = indices;
        }

        public void FreeBuffers(VertexPositionNormalTexCoordMaterial[] vertices, uint[] indices)
        {
            vertexPool.Return(vertices);
            indexPool.Return(indices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddFaceIndices(ref uint[] indices, ref uint indexCount, uint vertexStartOffset)
        {
            if (indexCount + 6 > indices.Length)
                ResizeIndexArray(ref indices);

            indices[indexCount++] = 0 + vertexStartOffset;
            indices[indexCount++] = 1 + vertexStartOffset;
            indices[indexCount++] = 2 + vertexStartOffset;
            indices[indexCount++] = 0 + vertexStartOffset;
            indices[indexCount++] = 2 + vertexStartOffset;
            indices[indexCount++] = 3 + vertexStartOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeVertexArray(ref VertexPositionNormalTexCoordMaterial[] vertices)
        {
            var newArray = vertexPool.Rent(vertices.Length * 2);
            Array.Copy(vertices, newArray, vertices.Length);
            vertexPool.Return(vertices);
            vertices = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIndexArray(ref uint[] indices)
        {
            var newArray = indexPool.Rent(indices.Length * 2);
            Array.Copy(indices, newArray, indices.Length);
            indexPool.Return(indices);
            indices = newArray;
        }
    }
}
