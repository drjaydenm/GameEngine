using System.Numerics;
using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.Content.Processors
{
    public class MeshProcessor : IContentProcessor<MeshRaw, Mesh<VertexPositionNormalTexCoordMaterial>>
    {
        public Mesh<VertexPositionNormalTexCoordMaterial> Process(MeshRaw contentRaw)
        {
            // TODO change the vertex format to use channels
            // TODO support more than one mesh piece
            var combinedVertices = new VertexPositionNormalTexCoordMaterial[contentRaw.Pieces[0].Vertices.Length];
            for (var i = 0; i < contentRaw.Pieces[0].Vertices.Length; i++)
            {
                combinedVertices[i] = new VertexPositionNormalTexCoordMaterial(
                    new Vector3(contentRaw.Pieces[0].Vertices[i].X, contentRaw.Pieces[0].Vertices[i].Y, contentRaw.Pieces[0].Vertices[i].Z),
                    new Vector3(contentRaw.Pieces[0].Normals[i].X, contentRaw.Pieces[0].Normals[i].Y, contentRaw.Pieces[0].Normals[i].Z),
                    new Vector2(contentRaw.Pieces[0].TexCoords[i].X, contentRaw.Pieces[0].TexCoords[i].Y),
                    1);
            }

            var mesh = new Mesh<VertexPositionNormalTexCoordMaterial>(combinedVertices,
                contentRaw.Pieces[0].Indices, contentRaw.Pieces[0].PrimitiveType);
            return mesh;
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((MeshRaw)contentRaw);
        }
    }
}
