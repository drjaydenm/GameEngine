namespace GameEngine.Core
{
    public class Mesh<T>
    {
        public struct Triangle
        {
            public T Vertex1 { get; set; }
            public T Vertex2 { get; set; }
            public T Vertex3 { get; set; }
        }

        public T[] Vertices { get; }
        public uint[] Indices { get; }

        public Mesh(T[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
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
    }
}
