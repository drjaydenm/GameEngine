﻿using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameEngine.Core.Content.Processors
{
    public abstract class TextureProcessor<TRaw> : IContentProcessor<TRaw, ITexture> where TRaw : IContentRaw
    {
        protected Engine Engine { get; }

        public TextureProcessor(Engine engine)
        {
            Engine = engine;
        }

        protected unsafe ITexture CreateTexture(Image<Rgba32>[] images)
        {
            var factory = Engine.GraphicsDevice.ResourceFactory;
            var format = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm;
            var mipLevels = 1;
            var pixelSizeInBytes = sizeof(byte) * 4;

            var tex = factory.CreateTexture(Veldrid.TextureDescription.Texture2D(
                (uint)images[0].Width, (uint)images[0].Height, (uint)mipLevels, (uint)images.Length, format, Veldrid.TextureUsage.Sampled));

            for (var layer = 0; layer < images.Length; layer++)
            {
                for (var level = 0; level < mipLevels; level++)
                {
                    var image = images[layer];
                    if (!image.DangerousTryGetSinglePixelMemory(out var pixelsMemory))
                    {
                        throw new InvalidOperationException("Could not get pixel span for texture");
                    }

                    using var pixelsHandle = pixelsMemory.Pin();

                    Engine.GraphicsDevice.UpdateTexture(
                        tex,
                        (IntPtr)pixelsHandle.Pointer,
                        (uint)(pixelSizeInBytes * image.Width * image.Height),
                        0,
                        0,
                        0,
                        (uint)image.Width,
                        (uint)image.Height,
                        1,
                        (uint)level,
                        (uint)layer);
                }
            }

            return tex;
        }

        public abstract ITexture Process(TRaw contentRaw);
        public abstract IContent Process(IContentRaw contentRaw);
    }

    public class Texture2DProcessor : TextureProcessor<Texture2DRaw>
    {
        public Texture2DProcessor(Engine engine) : base(engine)
        {
        }

        public override ITexture Process(Texture2DRaw contentRaw)
        {
            return CreateTexture(new[] { contentRaw.Image });
        }

        public override IContent Process(IContentRaw contentRaw)
        {
            return Process((Texture2DRaw)contentRaw);
        }
    }

    public class TextureArrayProcessor : TextureProcessor<TextureArrayRaw>
    {
        public TextureArrayProcessor(Engine engine) : base(engine)
        {
        }

        public override ITexture Process(TextureArrayRaw contentRaw)
        {
            return CreateTexture(contentRaw.Images);
        }

        public override IContent Process(IContentRaw contentRaw)
        {
            return Process((TextureArrayRaw)contentRaw);
        }
    }
}
