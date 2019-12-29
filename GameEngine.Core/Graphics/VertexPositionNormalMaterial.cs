using System.Numerics;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public struct VertexPositionNormalMaterial
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint MaterialId;

        public VertexPositionNormalMaterial(Vector3 position, Vector3 normal, uint materialId)
        {
            Position = position;
            Normal = normal;
            MaterialId = materialId;
        }

        public static readonly VertexLayoutDescription VertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("MaterialId", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));

        public const uint SizeInBytes = 28;
    }
}
