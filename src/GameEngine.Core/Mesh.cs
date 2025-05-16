using GameEngine.Core.Content;
using GameEngine.Core.Graphics;

namespace GameEngine.Core
{
    public class Mesh<T> : IContent
    {
        public struct Triangle
        {
            public T Vertex1 { get; set; }
            public T Vertex2 { get; set; }
            public T Vertex3 { get; set; }
        }

        public T[] Vertices;
        public uint[] Indices;
        public PrimitiveType PrimitiveType;

        public Mesh(T[] vertices, uint[] indices, PrimitiveType primitiveType)
        {
            Vertices = vertices;
            Indices = indices;
            PrimitiveType = primitiveType;
        }

        public Triangle[] TransformAsTriangles()
        {
            var triangles = new Triangle[Indices.Length / 3];

            for (int i = 0; i < Indices.Length / 3; ++i)
            {
                triangles[i] = new Triangle
                {
                    Vertex1 = Vertices[Indices[i * 3]],
                    Vertex2 = Vertices[Indices[i * 3 + 1]],
                    Vertex3 = Vertices[Indices[i * 3 + 2]]
                };
            }

            return triangles;
        }

        public void Dispose()
        {
        }
    }
}
