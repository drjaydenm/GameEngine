using System.Numerics;
using Veldrid;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Graphics
{
    public interface IRenderable : IComponent
    {
        Material Material { get; }
        VertexLayoutDescription LayoutDescription { get; }
        DeviceBuffer VertexBuffer { get; }
        DeviceBuffer IndexBuffer { get; }
        Matrix4x4 WorldTransform { get; }
        PrimitiveType PrimitiveType { get; }

        void UpdateBuffers(CommandList commandList);
    }
}
