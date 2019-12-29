using System.Numerics;
using GameEngine.Core.Entities;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class BasicRenderable : IRenderable
    {
        public Material Material { get; private set; }
        public VertexLayoutDescription LayoutDescription { get; private set; }
        public DeviceBuffer VertexBuffer { get; private set; }
        public DeviceBuffer IndexBuffer { get; private set; }
        public Matrix4x4 WorldTransform { get; private set; }

        private readonly Engine engine;

        public BasicRenderable(Engine engine, Material material)
        {
            this.engine = engine;

            Material = material;
        }

        public void SetMesh<T>(VertexLayoutDescription layoutDescription, Mesh<T> mesh) where T : struct
        {
            LayoutDescription = layoutDescription;

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            if (mesh == null || mesh.Vertices.Length <= 0)
                return;

            VertexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(layoutDescription.Stride * mesh.Vertices.Length), BufferUsage.VertexBuffer));
            IndexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(sizeof(uint) * mesh.Indices.Length), BufferUsage.IndexBuffer));
            engine.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, mesh.Vertices);
            engine.GraphicsDevice.UpdateBuffer(IndexBuffer, 0, mesh.Indices);
        }

        public void SetWorldTransform(Matrix4x4 world)
        {
            WorldTransform = world;
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
    }
}
