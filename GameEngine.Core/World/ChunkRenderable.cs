using System;
using System.Numerics;
using Veldrid;
using GameEngine.Core.Graphics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.World
{
    public class ChunkRenderable : IRenderable, IDisposable
    {
        public Material Material { get; private set; }
        public VertexLayoutDescription LayoutDescription => VertexPositionNormalTexCoordMaterial.VertexLayoutDescription;
        public DeviceBuffer VertexBuffer { get; private set; }
        public DeviceBuffer IndexBuffer { get; private set; }
        public Matrix4x4 WorldTransform { get; private set; }
        public PrimitiveType PrimitiveType => PrimitiveType.TriangleList;

        private readonly Engine engine;
        private readonly ChunkMeshGenerator meshGenerator;
        private VertexPositionNormalTexCoordMaterial[] vertices;
        private uint vertexCount;
        private uint[] indices;
        private uint indexCount;
        private bool isDirty;

        public ChunkRenderable(Chunk chunk, ChunkMeshGenerator meshGenerator, Engine engine, Material material)
        {
            this.engine = engine;
            this.meshGenerator = meshGenerator;

            Material = material;

            // Offset block vertices by half a block, as the block vertices are all centered around 0,0,0 instead
            // of 0,0,0 being the bottom corner of the block vertices
            WorldTransform = Matrix4x4.CreateTranslation(chunk.WorldPosition + (Chunk.CHUNK_SIZE * Chunk.CHUNK_BLOCK_RATIO * 0.5f));
        }

        public void UpdateChunk(VertexPositionNormalTexCoordMaterial[] vertices, uint vertexCount, uint[] indices, uint indexCount)
        {
            this.vertices = vertices;
            this.vertexCount = vertexCount;
            this.indices = indices;
            this.indexCount = indexCount;
            isDirty = true;
        }

        public void AttachedToEntity(Entity entity)
        {
        }

        public void DetachedFromEntity()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public unsafe void UpdateBuffers(CommandList commandList)
        {
            if (!isDirty)
                return;

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            if (vertexCount <= 0)
                return;

            var vertexBytes = (uint)(VertexPositionNormalTexCoordMaterial.SizeInBytes * vertexCount);
            var indexBytes = (uint)(sizeof(uint) * indexCount);
            VertexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(vertexBytes, BufferUsage.VertexBuffer));
            IndexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(indexBytes, BufferUsage.IndexBuffer));

            fixed (VertexPositionNormalTexCoordMaterial* pVertices = vertices)
            fixed(uint* pIndices = indices)
            {
                commandList.UpdateBuffer(VertexBuffer, 0, new IntPtr(pVertices), vertexBytes);
                commandList.UpdateBuffer(IndexBuffer, 0, new IntPtr(pIndices), indexBytes);
            }

            meshGenerator.FreeBuffers(vertices, indices);

            vertices = null;
            indices = null;
            isDirty = false;
        }
    }
}
