using System.Collections.Generic;
using System.Numerics;
using SharpText.Core;
using SharpText.Veldrid;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public struct TextRendererInfo
    {
        public ITextRenderer TextRenderer;
        public string FontPath;
        public float FontSize;
    }

    public class TextRenderer
    {
        private readonly Engine engine;
        private List<TextRendererInfo> textRenderers;

        public TextRenderer(Engine engine)
        {
            this.engine = engine;
            textRenderers = new List<TextRendererInfo>();
        }

        public void DrawText(string text, Vector2 coords, RgbaFloat color, string fontPath, float sizeInPixels)
        {
            var renderer = GetTextRenderer(fontPath, sizeInPixels);
            renderer.DrawText(text, coords, color.ToSharpTextColor());
        }

        public void Draw()
        {
            foreach (var renderer in textRenderers)
            {
                renderer.TextRenderer.Draw();
            }
        }

        public void Update()
        {
            foreach (var renderer in textRenderers)
            {
                renderer.TextRenderer.Update();
            }
        }

        private ITextRenderer GetTextRenderer(string fontPath, float sizeInPixels)
        {
            foreach (var renderer in textRenderers)
            {
                if (renderer.FontPath == fontPath && renderer.FontSize == sizeInPixels)
                    return renderer.TextRenderer;
            }

            var font = new Font(fontPath, sizeInPixels);
            var textRenderer = new VeldridTextRenderer(engine.GraphicsDevice, engine.CommandList, font);
            textRenderers.Add(new TextRendererInfo
            {
                TextRenderer = textRenderer,
                FontPath = fontPath,
                FontSize = sizeInPixels
            });

            return textRenderer;
        }
    }
}
