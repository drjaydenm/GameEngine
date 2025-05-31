using System.Numerics;
using GameEngine.Core.Entities;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class BasicRenderable<T>: IRenderable, IDisposable where T : unmanaged
    {
        public Material Material { get; private set; }
        public VertexLayoutDescription LayoutDescription { get; private set; }
        public DeviceBuffer VertexBuffer { get; private set; }
        public DeviceBuffer IndexBuffer { get; private set; }
        public PrimitiveType PrimitiveType => mesh.PrimitiveType;
        public Vector3 PositionOffset { get; set; }

        private readonly Engine engine;
        private Mesh<T> mesh;
        private bool isDirty;

        public BasicRenderable(Engine engine, Material material)
        {
            this.engine = engine;

            Material = material;
        }

        public void SetMesh(VertexLayoutDescription layoutDescription, Mesh<T> mesh)
        {
            LayoutDescription = layoutDescription;

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

        public void UpdateBuffers(ICommandList commandList)
        {
            if (!isDirty)
                return;

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            if (mesh.Vertices == null || mesh.Vertices.Length <= 0)
                return;

            VertexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(LayoutDescription.Stride * mesh.Vertices.Length), BufferUsage.VertexBuffer));
            IndexBuffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)(sizeof(uint) * mesh.Indices.Length), BufferUsage.IndexBuffer));
            engine.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, mesh.Vertices);
            engine.GraphicsDevice.UpdateBuffer(IndexBuffer, 0, mesh.Indices);

            isDirty = false;
        }
    }
}
