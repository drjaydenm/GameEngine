using System.IO;
using GameEngine.Core.Content.Raw;
using Newtonsoft.Json;

namespace GameEngine.Core.Content.Importers
{
    public class ShaderImporter : IContentImporter<ShaderRaw>
    {
        public string[] FileExtensions => new[] { ".shader" };

        public ShaderRaw Import(string filePath)
        {
            var vertexPath = Path.ChangeExtension(filePath, "vert");
            var fragmentPath = Path.ChangeExtension(filePath, "frag");
            var configPath = Path.ChangeExtension(filePath, "shaderconfig.json");

            ShaderRaw shader;
            using (var fsVert = new StreamReader(vertexPath))
            using (var fsFrag = new StreamReader(fragmentPath))
            using (var fsConfig = new StreamReader(configPath))
            {
                var config = JsonConvert.DeserializeObject<ShaderConfigRaw>(fsConfig.ReadToEnd());
                
                shader = new ShaderRaw(fsVert.ReadToEnd(), fsFrag.ReadToEnd(), config);
            }

            return shader;
        }

        IContentRaw IContentImporter.Import(string filePath)
        {
            return Import(filePath);
        }
    }
}
