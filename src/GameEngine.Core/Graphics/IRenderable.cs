using System.Numerics;
using Veldrid;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Graphics
{
    public interface IRenderable : IComponent
    {
        Material Material { get; }
        VertexLayoutDescription LayoutDescription { get; }
        IBuffer VertexBuffer { get; }
        IBuffer IndexBuffer { get; }
        PrimitiveType PrimitiveType { get; }
        Vector3 PositionOffset { get; }

        void UpdateBuffers(ICommandList commandList);
    }
}
