using System;
using System.IO;
using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Importers
{
    public class TextureArrayAsset
    {
        public string Type { get; set; }
        public string[] Paths { get; set; }
    }

    public class TextureArrayImporter : IContentImporter<TextureArrayRaw>
    {
        public string[] FileExtensions => new[] { "texarray.json" };

        public TextureArrayRaw Import(string filePath)
        {
            TextureArrayRaw texArray;
            using (var sr = new StreamReader(filePath))
            {
                var asset = JsonConvert.DeserializeObject<TextureArrayAsset>(sr.ReadToEnd());
                var textureType = (TextureType)Enum.Parse(typeof(TextureType), "Texture" + asset.Type + "Array");

                texArray = new TextureArrayRaw(textureType, asset.Paths.Length);

                var lastWidth = -1;
                var lastHeight = -1;
                for (var i = 0; i < asset.Paths.Length; i++)
                {
                    var texturePath = Path.Combine(Path.GetDirectoryName(filePath), asset.Paths[i]);
                    using (var fs = new FileStream(texturePath, FileMode.Open, FileAccess.Read))
                    {
                        var image = Image.Load<Rgba32>(fs);

                        if ((lastWidth != -1 && lastWidth != image.Width)
                            || (lastHeight != -1 && lastHeight != image.Height))
                        {
                            throw new Exception("Texture arrays cannot contain images of different resolutions");
                        }
                        lastWidth = image.Width;
                        lastHeight = image.Height;

                        texArray.Images[i] = image;
                    }
                }
            }

            return texArray;
        }

        IContentRaw IContentImporter.Import(string filePath)
        {
            return Import(filePath);
        }
    }
}
