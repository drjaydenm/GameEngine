using System.IO;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public class ShaderImporter : IContentImporter<ShaderRaw>
    {
        public string[] FileExtensions => new[] { ".shader" };

        public ShaderRaw Import(string filePath)
        {
            var vertexPath = Path.ChangeExtension(filePath, "vert");
            var fragmentPath = Path.ChangeExtension(filePath, "frag");

            ShaderRaw shader;
            using (var fsVert = new StreamReader(vertexPath))
            using (var fsFrag = new StreamReader(fragmentPath))
            {
                shader = new ShaderRaw(fsVert.ReadToEnd(), fsFrag.ReadToEnd());
            }

            return shader;
        }

        IContentRaw IContentImporter.Import(string filePath)
        {
            return Import(filePath);
        }
    }
}
