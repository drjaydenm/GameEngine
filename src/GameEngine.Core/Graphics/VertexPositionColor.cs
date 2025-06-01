using System.Numerics;

namespace GameEngine.Core.Graphics
{
    public struct VertexPositionColor
    {
        public Vector3 Position;
        public Color Color;

        public VertexPositionColor(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public static readonly VertexLayoutDescription VertexLayoutDescription = new()
        {
            Stride = SizeInBytes,
            Elements =
            [
                new VertexElementDescription("Position", VertexElementFormat.Float3, 0),
                new VertexElementDescription("Color", VertexElementFormat.Float4, 12)
            ]
        };

        public const uint SizeInBytes = 28;
    }
}
