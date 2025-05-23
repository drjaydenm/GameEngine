using Assimp;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public class MeshImporter : IContentImporter<MeshRaw>
    {
        public string[] FileExtensions => new[] { ".fbx" };

        public MeshRaw Import(IContentLoader loader, string filePath)
        {
            var context = new AssimpContext();

            using var stream = loader.OpenStream(filePath);
            var scene = context.ImportFileFromStream(stream,
                PostProcessSteps.GenerateNormals | PostProcessSteps.GenerateUVCoords
                | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.JoinIdenticalVertices
                | PostProcessSteps.Triangulate | PostProcessSteps.FlipWindingOrder
                | PostProcessSteps.FixInFacingNormals);

            var meshRaw = new MeshRaw();

            foreach (var mesh in scene.Meshes)
            {
                var vertices = mesh.Vertices;
                var indices = mesh.GetUnsignedIndices();
                var texCoords = mesh.TextureCoordinateChannels[0];
                var normals = mesh.Normals;

                meshRaw.Pieces.Add(new MeshPieceRaw(vertices.ToArray(), indices, texCoords.ToArray(),
                    normals.ToArray(), AssimpPrimitiveTypeToEngine(mesh.PrimitiveType)));
            }

            context.Dispose();

            return meshRaw;
        }

        IContentRaw IContentImporter.Import(IContentLoader loader, string filePath)
        {
            return Import(loader, filePath);
        }

        private Graphics.PrimitiveType AssimpPrimitiveTypeToEngine(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Line:
                    return Graphics.PrimitiveType.LineList;
                case PrimitiveType.Triangle:
                    return Graphics.PrimitiveType.TriangleList;
                case PrimitiveType.Polygon:
                    return Graphics.PrimitiveType.TriangleStrip;
                default:
                    throw new Exception("Unknown primitive type");
            }
        }
    }
}
