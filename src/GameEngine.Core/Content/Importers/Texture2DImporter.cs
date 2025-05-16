using GameEngine.Core.Content.Raw;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Importers
{
    public class Texture2DImporter : IContentImporter<Texture2DRaw>
    {
        public string[] FileExtensions => new[] { ".jpg", ".jpeg" };

        public Texture2DRaw Import(IContentLoader loader, string filePath)
        {
            using var s = loader.OpenStream(filePath);
            var image = Image.Load<Rgba32>(s);

            return new Texture2DRaw(image);
        }

        IContentRaw IContentImporter.Import(IContentLoader loader, string filePath)
        {
            return Import(loader, filePath);
        }
    }
}
