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

            engine.Window.Resized += (object sender, EventArgs e) => OnResize();
        }

        public void DrawText(string text, Vector2 coords, RgbaFloat color, string fontPath, float sizeInPixels)
        {
            var renderer = GetTextRenderer(fontPath, sizeInPixels);
            renderer.DrawText(text, coords, color.ToSharpTextColor());
        }

        public void Draw()
        {
            for (var i = 0; i < textRenderers.Count; i++)
            {
                textRenderers[i].TextRenderer.Draw();
            }
        }

        public void Update()
        {
            for (var i = 0; i < textRenderers.Count; i++)
            {
                textRenderers[i].TextRenderer.Update();
            }
        }

        private ITextRenderer GetTextRenderer(string fontPath, float sizeInPixels)
        {
            for (var i = 0; i < textRenderers.Count; i++)
            {
                if (textRenderers[i].FontPath == fontPath && textRenderers[i].FontSize == sizeInPixels)
                    return textRenderers[i].TextRenderer;
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

        private void OnResize()
        {
            for (var i = 0; i < textRenderers.Count; i++)
            {
                textRenderers[i].TextRenderer.ResizeToSwapchain();
            }
        }
    }
}
