using System;
using System.Runtime.InteropServices;
using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;
using SixLabors.ImageSharp.Advanced;
using Veldrid;

namespace GameEngine.Core.Content.Processors
{
    public class Texture2DProcessor : IContentProcessor<Texture2DRaw, Texture2D>
    {
        private readonly Engine engine;

        public Texture2DProcessor(Engine engine)
        {
            this.engine = engine;
        }

        public unsafe Texture2D Process(Texture2DRaw contentRaw)
        {
            var factory = engine.GraphicsDevice.ResourceFactory;
            var format = PixelFormat.R8_G8_B8_A8_UNorm;
            var mipLevels = 1;
            var images = new[] { contentRaw.Image };
            var pixelSizeInBytes = sizeof(byte) * 4;

            var tex = factory.CreateTexture(TextureDescription.Texture2D(
                (uint)contentRaw.Image.Width, (uint)contentRaw.Image.Height, (uint)mipLevels, 1, format, TextureUsage.Sampled));

            for (var level = 0; level < mipLevels; level++)
            {
                var image = images[level];
                fixed (void* pin = &MemoryMarshal.GetReference(image.GetPixelSpan()))
                {
                    engine.GraphicsDevice.UpdateTexture(
                        tex,
                        (IntPtr)pin,
                        (uint)(pixelSizeInBytes * image.Width * image.Height),
                        0,
                        0,
                        0,
                        (uint)image.Width,
                        (uint)image.Height,
                        1,
                        (uint)level,
                        0);
                }
            }

            return new Texture2D(tex);
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((Texture2DRaw)contentRaw);
        }
    }
}
