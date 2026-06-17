using System.Numerics;
using SharpText.Core;

namespace GameEngine.Core.Rendering
{
    public struct TextRendererInfo
    {
        public ITextRenderer TextRenderer;
        public string FontPath;
        public float FontSize;
    }

    public class TextRenderer
    {
        private readonly Engine _engine;
        private List<TextRendererInfo> _textRenderers;

        public TextRenderer(Engine engine)
        {
            this._engine = engine;
            _textRenderers = new List<TextRendererInfo>();

            engine.Window.Resized += (object sender, EventArgs e) => OnResize();
        }

        public void DrawText(string text, Vector2 coords, Color color, string fontPath, float sizeInPixels)
        {
            var renderer = GetTextRenderer(fontPath, sizeInPixels);
            renderer.DrawText(text, coords, new SharpText.Core.Color(color.R, color.G, color.B, color.A));
        }

        public void Draw()
        {
            for (var i = 0; i < _textRenderers.Count; i++)
            {
                _textRenderers[i].TextRenderer.Draw();
            }
        }

        public void Update()
        {
            for (var i = 0; i < _textRenderers.Count; i++)
            {
                _textRenderers[i].TextRenderer.Update();
            }
        }

        private ITextRenderer GetTextRenderer(string fontPath, float sizeInPixels)
        {
            for (var i = 0; i < _textRenderers.Count; i++)
            {
                if (_textRenderers[i].FontPath == fontPath && _textRenderers[i].FontSize == sizeInPixels)
                    return _textRenderers[i].TextRenderer;
            }

            var font = new Font(fontPath, sizeInPixels);
            var textRenderer = new SharpTextRenderer(_engine.GraphicsDevice, _engine.CommandList, font);

            _textRenderers.Add(new TextRendererInfo
            {
                TextRenderer = textRenderer,
                FontPath = fontPath,
                FontSize = sizeInPixels
            });

            return textRenderer;
        }

        private void OnResize()
        {
            for (var i = 0; i < _textRenderers.Count; i++)
            {
                _textRenderers[i].TextRenderer.ResizeToSwapchain();
            }
        }
    }
}
