using System.IO;
using GameEngine.Core.Content.Raw;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Importers
{
    public class Texture2DImporter : IContentImporter<Texture2DRaw>
    {
        public string[] FileExtensions => new[] { ".jpg", ".jpeg" };

        public Texture2DRaw Import(string filePath)
        {
            Image<Rgba32> image;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                image = Image.Load<Rgba32>(fs);
            }

            return new Texture2DRaw(image);
        }

        IContentRaw IContentImporter.Import(string filePath)
        {
            return Import(filePath);
        }
    }
}
