using Assimp;

namespace GameEngine.Core.Content.Raw
{
    public class MeshPieceRaw
    {
        public Vector3D[] Vertices { get; }
        public uint[] Indices { get; }
        public Vector3D[] TexCoords { get; }
        public Vector3D[] Normals { get; }
        public Graphics.PrimitiveType PrimitiveType { get; }

        public MeshPieceRaw(Vector3D[] vertices, uint[] indices, Vector3D[] texCoords,
            Vector3D[] normals, Graphics.PrimitiveType primitiveType)
        {
            Vertices = vertices;
            Indices = indices;
            TexCoords = texCoords;
            Normals = normals;
            PrimitiveType = primitiveType;
        }
    }
}
