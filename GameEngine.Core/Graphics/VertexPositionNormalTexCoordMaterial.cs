using System.Numerics;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public struct VertexPositionNormalTexCoordMaterial
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public uint MaterialId;

        public VertexPositionNormalTexCoordMaterial(Vector3 position, Vector3 normal, Vector2 texCoord, uint materialId)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
            MaterialId = materialId;
        }

        public static readonly VertexLayoutDescription VertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("MaterialId", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));

        public const uint SizeInBytes = 36;
    }
}
