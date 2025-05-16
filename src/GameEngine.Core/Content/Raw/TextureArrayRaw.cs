using GameEngine.Core.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Raw
{
    public class TextureArrayRaw : IContentRaw
    {
        public TextureType Type { get; }
        public Image<Rgba32>[] Images { get; }

        public TextureArrayRaw(TextureType texType, int imageCount)
        {
            Type = texType;
            Images = new Image<Rgba32>[imageCount];
        }
    }
}
