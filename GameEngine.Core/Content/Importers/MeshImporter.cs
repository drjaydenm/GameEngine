using System;
using Assimp;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public class MeshImporter : IContentImporter<MeshRaw>
    {
        public string[] FileExtensions => new[] { ".fbx" };

        public MeshRaw Import(string filePath)
        {
            var context = new AssimpContext();
            var scene = context.ImportFile(filePath);

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

        IContentRaw IContentImporter.Import(string filePath)
        {
            return Import(filePath);
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
