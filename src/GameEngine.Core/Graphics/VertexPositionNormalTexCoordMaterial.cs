using System.Numerics;

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

        public static readonly VertexLayoutDescription VertexLayoutDescription = new()
        {
            Stride = SizeInBytes,
            Elements =
            [
                new VertexElementDescription("Position", VertexElementFormat.Float3, 0),
                new VertexElementDescription("Normal", VertexElementFormat.Float3, 12),
                new VertexElementDescription("TexCoord", VertexElementFormat.Float2, 24),
                new VertexElementDescription("MaterialId", VertexElementFormat.UInt1, 32)
            ]
        };

        public const uint SizeInBytes = 36;
    }
}
