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
        public VertexLayoutDescription LayoutDescription => VertexPositionNormalMaterial.VertexLayoutDescription;
        public DeviceBuffer VertexBuffer { get; private set; }
        public DeviceBuffer IndexBuffer { get; private set; }
        public Matrix4x4 WorldTransform { get; private set; }

        private readonly Engine engine;
        private Mesh<VertexPositionNormalMaterial> mesh;
        private bool isDirty;

        public ChunkRenderable(Chunk chunk, Engine engine, Material material)
        {
            this.engine = engine;

            Material = material;

            // Offset block vertices by half a block, as the block vertices are all centered around 0,0,0 instead
            // of 0,0,0 being the bottom corner of the block vertices
            WorldTransform = Matrix4x4.CreateTranslation(chunk.WorldPosition + (Chunk.CHUNK_SIZE * Chunk.CHUNK_BLOCK_RATIO * 0.5f));
        }

        public void UpdateChunk(Mesh<VertexPositionNormalMaterial> mesh)
        {
            this.mesh = mesh;
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

        public void UpdateBuffers(CommandList commandList)
        {
            if (!isDirty)
                return;

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            if (mesh == null || mesh.Vertices.Length <= 0)
                return;

            VertexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(VertexPositionNormalMaterial.SizeInBytes * mesh.Vertices.Length), BufferUsage.VertexBuffer));
            IndexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(sizeof(uint) * mesh.Indices.Length), BufferUsage.IndexBuffer));
            commandList.UpdateBuffer(VertexBuffer, 0, mesh.Vertices);
            commandList.UpdateBuffer(IndexBuffer, 0, mesh.Indices);

            mesh = null;
            isDirty = false;
        }
    }
}
