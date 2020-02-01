using System.IO;
using GameEngine.Core.Content.Raw;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Importers
{
    public class Texture2DImporter : IContentImporter<Texture2DRaw>
    {
        public string[] FileExtensions => new[] { ".jpg", ".jpeg" };

        public Texture2DRaw Import(Stream stream)
        {
            var image = Image.Load<Rgba32>(stream);

            return new Texture2DRaw(image);
        }

        IContentRaw IContentImporter.Import(Stream stream)
        {
            return Import(stream);
        }
    }
}
