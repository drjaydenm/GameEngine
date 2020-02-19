using System.Numerics;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public struct VertexPositionNormalTexCoord
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;

        public VertexPositionNormalTexCoord(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }

        public static readonly VertexLayoutDescription VertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

        public const uint SizeInBytes = 32;
    }
}
