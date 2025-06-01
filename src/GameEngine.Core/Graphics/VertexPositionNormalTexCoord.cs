using System.Numerics;

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

        public static readonly VertexLayoutDescription VertexLayoutDescription = new()
        {
            Stride = SizeInBytes,
            Elements =
            [
                new VertexElementDescription("Position", VertexElementFormat.Float3, 0),
                new VertexElementDescription("Normal", VertexElementFormat.Float3, 12),
                new VertexElementDescription("TexCoord", VertexElementFormat.Float2, 24)
            ]
        };

        public const uint SizeInBytes = 32;
    }
}
