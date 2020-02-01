using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Raw
{
    public class Texture2DRaw : IContentRaw
    {
        public Image<Rgba32> Image { get; }

        public Texture2DRaw(Image<Rgba32> image)
        {
            Image = image;
        }
    }
}
