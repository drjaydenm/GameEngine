using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GameEngine.Core.Graphics;
using SharpText.Core;

namespace GameEngine.Core.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal struct TextVertexProperties
{
    public Matrix4x4 Transform;
    public Vector4 Rectangle;
}

[StructLayout(LayoutKind.Sequential)]
internal struct TextFragmentProperties
{
    public float ThicknessAndMode;
    private float _padding1;
    private float _padding2;
    private float _padding3;
    public Color GlyphColor;
}

internal struct DrawableText
{
    public string Text;
    public DrawableGlyph[] Glyphs;
    public Vector2 Position;
    public Color Color;
    public float LetterSpacing;
    public BoundingRectangle Rectangle;
}

internal struct DrawableGlyph
{
    public VertexPosition3Coord2[] Vertices;
    public float AdvanceX;
}

internal class SharpTextRenderer : ITextRenderer
{
    public Font Font { get; private set; }

    private readonly IGraphicsDevice graphicsDevice;
    private readonly ICommandList commandList;
    private IBuffer glyphVertexBuffer;
    private IBuffer quadVertexBuffer;
    private IBuffer textVertexPropertiesBuffer;
    private IBuffer textFragmentPropertiesBuffer;
    private IPipeline outputPipeline;
    private IPipeline outputColorPipeline;
    private IPipeline glyphPipeline;
    private VertexPosition2[] quadVertices;
    private TextVertexProperties textVertexProperties;
    private TextFragmentProperties textFragmentProperties;
    private IResourceLayout textTextureLayout;
    private IResourceSet textPropertiesSet;
    private IResourceSet textTextureSet;
    private IResourceSet dummyTextureSet;
    private ITexture glyphTexture;
    private ITextureView glyphTextureView;
    private IFramebuffer glyphTextureFramebuffer;
    private ITexture dummyTexture;
    private ITextureView dummyTextureView;
    private Shader glyphShader;
    private Shader textShader;
    private Vector2[] jitterPattern = {
        new Vector2(-1 / 12f, -5 / 12f),
        new Vector2( 1 / 12f,  1 / 12f),
        new Vector2( 3 / 12f, -1 / 12f),
        new Vector2( 5 / 12f,  5 / 12f),
        new Vector2( 7 / 12f, -3 / 12f),
        new Vector2( 9 / 12f,  3 / 12f)
    };
    private List<DrawableText> textToDraw;
    private Dictionary<char, DrawableGlyph> cachedGlyphs;
    private float aspectWidth;
    private float aspectHeight;

    public SharpTextRenderer(IGraphicsDevice graphicsDevice, ICommandList commandList, Font font)
    {
        this.graphicsDevice = graphicsDevice;
        this.commandList = commandList;
        Font = font;

        textToDraw = new List<DrawableText>();
        cachedGlyphs = new Dictionary<char, DrawableGlyph>();

        Initialize();
    }

    public void UpdateFont(Font font)
    {
        Font = font;
        cachedGlyphs.Clear();
    }

    public void DrawText(string text, Vector2 coordsInPixels, SharpText.Core.Color color, float letterSpacing)
    {
        var drawable = new DrawableText
        {
            Text = text,
            Glyphs = new DrawableGlyph[text.Length],
            Position = coordsInPixels,
            Color = new Color(color.R, color.G, color.B, color.A),
            LetterSpacing = letterSpacing
        };

        var measurementInfo = Font.GetMeasurementInfoForString(text);
        var accumulatedAdvanceWidths = 0f;
        drawable.Rectangle.Reset();
        var stringVertices = Font.GetVerticesForString(text);

        for (var i = 0; i < text.Length; i++)
        {
            if (cachedGlyphs.ContainsKey(text[i]))
            {
                drawable.Glyphs[i] = cachedGlyphs[text[i]];
                accumulatedAdvanceWidths += measurementInfo.AdvanceWidths[i];
                continue;
            }

            var vertices = stringVertices.Vertices[i];

            // Extend the text rectangle to contain this letter
            for (var j = 0; j < vertices.Length; j++)
            {
                drawable.Rectangle.Include(vertices[j].Position.X + accumulatedAdvanceWidths, vertices[j].Position.Y);
            }

            var drawableGlyph = new DrawableGlyph
            {
                Vertices = vertices,
                AdvanceX = measurementInfo.AdvanceWidths[i]
            };
            drawable.Glyphs[i] = drawableGlyph;
            cachedGlyphs.Add(text[i], drawableGlyph);

            accumulatedAdvanceWidths += measurementInfo.AdvanceWidths[i];
        }

        textToDraw.Add(drawable);
    }

    public void Draw()
    {
        var textGroupedByColor = textToDraw.GroupBy(t => t.Color);

        // Render text sets by color
        foreach (var colorGroup in textGroupedByColor)
        {
            commandList.SetPipeline(glyphPipeline);
            commandList.SetGraphicsResourceSet(0, textPropertiesSet);
            commandList.SetFramebuffer(glyphTextureFramebuffer);

            commandList.ClearColorTarget(0, Color.Transparent);

            var updateRect = new BoundingRectangle();
            updateRect.Reset();
            foreach (var drawable in colorGroup)
            {
                var advanceX = 0f;
                for (var i = 0; i < drawable.Glyphs.Length; i++)
                {
                    var glyphAdvanceX = drawable.Glyphs[i].AdvanceX;
                    // Transform letter spacing from 1 based to 0 based
                    glyphAdvanceX += (drawable.LetterSpacing - 1) * Font.FontSizeInPixels;

                    var glyphCoordsInPixels = new Vector2(advanceX, 0) + drawable.Position;
                    DrawGlyph(commandList, drawable.Glyphs[i].Vertices, glyphCoordsInPixels);
                    advanceX += glyphAdvanceX;
                }

                updateRect.Include(drawable.Position.X * aspectWidth / 2f,
                    drawable.Position.Y * aspectHeight / 2f);
                updateRect.Include((drawable.Position.X + advanceX) * aspectWidth / 2f,
                    (drawable.Position.Y + Font.FontSizeInPixels) * aspectHeight / 2f);
            }

            const float paddingPixels = 1;
            updateRect.Include(updateRect.StartX - (paddingPixels * aspectWidth / 2f),
                updateRect.StartY - (paddingPixels * aspectHeight / 2f));
            updateRect.Include(updateRect.EndX + (paddingPixels * aspectWidth / 2f),
                updateRect.EndY + (paddingPixels * aspectHeight / 2f));

            // 2nd pass, render everything to the framebuffer
            DrawOutput(commandList, new Color(0, 0, 0, 0), false, updateRect.ToVector4());

            // We need to do a second pass if we would like a color other than black text
            if (colorGroup.Key != new Color(0, 0, 0, 1))
            {
                DrawOutput(commandList, colorGroup.Key, true, updateRect.ToVector4());
            }
        }
    }

    public void Update()
    {
        textToDraw.Clear();
    }

    public void Dispose()
    {
        outputPipeline.Dispose();
        outputColorPipeline.Dispose();
        glyphPipeline.Dispose();

        textPropertiesSet.Dispose();
        textTextureSet.Dispose();
        dummyTextureSet.Dispose();

        glyphShader.Dispose();
        textShader.Dispose();

        glyphTexture.Dispose();
        glyphTextureView.Dispose();
        glyphTextureFramebuffer.Dispose();
        dummyTexture.Dispose();
        dummyTextureView.Dispose();

        glyphVertexBuffer.Dispose();
        quadVertexBuffer.Dispose();
        textVertexPropertiesBuffer.Dispose();
        textFragmentPropertiesBuffer.Dispose();
    }

    public void ResizeToSwapchain()
    {
        aspectWidth = 2f / graphicsDevice.SwapchainFramebuffer.Width;
        aspectHeight = 2f / graphicsDevice.SwapchainFramebuffer.Height;

        var factory = graphicsDevice.ResourceFactory;

        glyphTexture?.Dispose();
        glyphTextureView?.Dispose();
        glyphTextureFramebuffer?.Dispose();
        textTextureSet.Dispose();

        var colorFormat = graphicsDevice.SwapchainFramebuffer.OutputDescription.ColorAttachments[0].Format;
        glyphTexture = factory.CreateTexture(new TextureDescription
        {
            Width = graphicsDevice.SwapchainFramebuffer.Width,
            Height = graphicsDevice.SwapchainFramebuffer.Height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = colorFormat,
            Usage = TextureUsage.RenderTarget | TextureUsage.Sampled,
            Type = TextureType.Texture2D,
            SampleCount = TextureSampleCount.Count1
        });
        glyphTextureView = factory.CreateTextureView(glyphTexture);
        glyphTextureFramebuffer = factory.CreateFramebuffer(new FramebufferDescription
        {
            ColorTargets = [
                new FramebufferAttachmentDescription { Target = glyphTexture }
            ]
        });
        textTextureSet = factory.CreateResourceSet(new ResourceSetDescription
        {
            Layout = textTextureLayout,
            Resources = [ glyphTextureView, graphicsDevice.LinearSampler ]
        });
    }

    private void DrawGlyph(ICommandList commandList, VertexPosition3Coord2[] glyphVertices, Vector2 coordsInPixels)
    {
        // Resize the vertex buffer if required
        var requiredBufferSize = VertexPosition3Coord2.SizeInBytes * (uint)glyphVertices.Length;
        if (glyphVertexBuffer.SizeInBytes < requiredBufferSize)
        {
            glyphVertexBuffer.Dispose();
            glyphVertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(requiredBufferSize, BufferUsage.VertexBuffer));
        }

        commandList.UpdateBuffer(glyphVertexBuffer, 0, glyphVertices);
        commandList.SetVertexBuffer(0, glyphVertexBuffer);

        var coordsInScreenSpace = coordsInPixels * new Vector2(aspectWidth, aspectHeight);
        var textTransformMatrix = Matrix4x4.CreateScale(aspectWidth, aspectHeight, 1)
            * Matrix4x4.CreateTranslation(-1, 0, 0)
            * Matrix4x4.CreateTranslation(coordsInScreenSpace.X, 1f - coordsInScreenSpace.Y, 0);

        for (var i = 0; i < jitterPattern.Length; i++)
        {
            var jitter = jitterPattern[i];

            var glyphTransformMatrix = Matrix4x4.CreateTranslation(new Vector3(jitter, 0)) * textTransformMatrix;
            textVertexProperties.Transform = glyphTransformMatrix;
            commandList.UpdateBuffer(textVertexPropertiesBuffer, 0, textVertexProperties);

            if (i % 2 == 0)
            {
                textFragmentProperties.GlyphColor = new Color(i == 0 ? 1 : 0, i == 2 ? 1 : 0, i == 4 ? 1 : 0, 0);
                commandList.UpdateBuffer(textFragmentPropertiesBuffer, 0, textFragmentProperties);
            }

            commandList.Draw((uint)glyphVertices.Length, 1, 0, 0);
        }
    }

    private void DrawOutput(ICommandList commandList, Color color, bool secondPass, Vector4 rect)
    {
        textVertexProperties.Rectangle = rect;
        commandList.UpdateBuffer(textVertexPropertiesBuffer, 0, textVertexProperties);

        textFragmentProperties.GlyphColor = color;
        commandList.UpdateBuffer(textFragmentPropertiesBuffer, 0, textFragmentProperties);

        commandList.SetPipeline(secondPass ? outputColorPipeline : outputPipeline);
        commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
        commandList.SetGraphicsResourceSet(0, textPropertiesSet);
        // HACK workaround issue with texture view caching for shader resources
        commandList.SetGraphicsResourceSet(1, dummyTextureSet);
        commandList.SetGraphicsResourceSet(1, textTextureSet);
        commandList.SetVertexBuffer(0, quadVertexBuffer);
        commandList.Draw((uint)quadVertices.Length);
    }

    private void Initialize()
    {
        var factory = graphicsDevice.ResourceFactory;
        aspectWidth = 2f / graphicsDevice.SwapchainFramebuffer.Width;
        aspectHeight = 2f / graphicsDevice.SwapchainFramebuffer.Height;

        glyphVertexBuffer = factory.CreateBuffer(new BufferDescription(VertexPosition3Coord2.SizeInBytes, BufferUsage.VertexBuffer));
        quadVertexBuffer = factory.CreateBuffer(new BufferDescription(VertexPosition2.SizeInBytes * 4, BufferUsage.VertexBuffer));
        quadVertices =
        [
            new VertexPosition2(new Vector2(-1, 1)),
            new VertexPosition2(new Vector2(-1, -1)),
            new VertexPosition2(new Vector2(1, 1)),
            new VertexPosition2(new Vector2(1, -1))
        ];
        graphicsDevice.UpdateBuffer(quadVertexBuffer, 0, quadVertices);

        textVertexPropertiesBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<TextVertexProperties>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        textFragmentPropertiesBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<TextFragmentProperties>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        textVertexProperties = new TextVertexProperties
        {
            Transform = new Matrix4x4(),
            Rectangle = new Vector4(0, 0, 1, 1)
        };
        textFragmentProperties = new TextFragmentProperties
        {
            ThicknessAndMode = 0, // TODO support other modes
            GlyphColor = new Color(0, 0.5f, 1, 1)
        };
        graphicsDevice.UpdateBuffer(textVertexPropertiesBuffer, 0, [textVertexProperties]);
        graphicsDevice.UpdateBuffer(textFragmentPropertiesBuffer, 0, [textFragmentProperties]);

        var colorFormat = graphicsDevice.SwapchainFramebuffer.OutputDescription.ColorAttachments[0].Format;
        glyphTexture = factory.CreateTexture(new TextureDescription
        {
            Width = graphicsDevice.SwapchainFramebuffer.Width,
            Height = graphicsDevice.SwapchainFramebuffer.Height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = colorFormat,
            Usage = TextureUsage.RenderTarget | TextureUsage.Sampled,
            Type = TextureType.Texture2D,
            SampleCount = TextureSampleCount.Count1
        });
        glyphTextureView = factory.CreateTextureView(glyphTexture);
        glyphTextureFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(glyphTexture)));

        // HACK workaround issue with texture view caching for shader resources
        dummyTexture = factory.CreateTexture(new TextureDescription(1, 1, 1, 1, 1,
            colorFormat, TextureUsage.Sampled, TextureType.Texture2D, TextureSampleCount.Count1));
        dummyTextureView = factory.CreateTextureView(dummyTexture);

        CompileShaders();

        var textPropertiesLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("TextVertexPropertiesBuffer", ResourceType.UniformBuffer, ShaderStage.Vertex),
                new ResourceLayoutElementDescription("TextFragmentPropertiesBuffer", ResourceType.UniformBuffer, ShaderStage.Fragment)));

        textTextureLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("GlyphTexture", ResourceType.TextureReadOnly, ShaderStage.Fragment),
                new ResourceLayoutElementDescription("GlyphTextureSampler", ResourceType.Sampler, ShaderStage.Fragment)));

        textPropertiesSet = factory.CreateResourceSet(new ResourceSetDescription
        {
            Layout = textPropertiesLayout,
            Resources = [ textVertexPropertiesBuffer, textFragmentPropertiesBuffer ]
        });

        textTextureSet = factory.CreateResourceSet(new ResourceSetDescription
        {
            Layout = textTextureLayout,
            Resources = [glyphTextureView, graphicsDevice.LinearSampler]
        });
        // HACK workaround issue with texture view caching for shader resources
        dummyTextureSet = factory.CreateResourceSet(new ResourceSetDescription
        {
            Layout = textTextureLayout,
            Resources = [ dummyTextureView, graphicsDevice.LinearSampler ]
        });

        var additiveBlendState = new BlendStateDescription(Color.White,
            new BlendAttachmentDescription(
                blendEnabled: true,
                colorWriteMask: ColorWriteMask.All,
                sourceColorFactor: BlendFactor.One,
                destinationColorFactor: BlendFactor.One,
                colorFunction: BlendFunction.Add,
                sourceAlphaFactor: BlendFactor.One,
                destinationAlphaFactor: BlendFactor.One,
                alphaFunction: BlendFunction.Add), false);
        var depthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            depthComparisonType: ComparisonType.LessEqual);
        var outputShaderSet = new ShaderSetDescription(
            vertexLayouts:
            [
                new VertexLayoutDescription
                {
                    Stride = 8,
                    Elements =
                    [
                        new VertexElementDescription("Position", VertexElementFormat.Float2, 0)
                    ]
                }
            ],
            shader: textShader,
            specializations: []);

        var outputPipelineDescription = new GraphicsPipelineDescription(
            blendState: new BlendStateDescription(Color.White,
                new BlendAttachmentDescription(
                    blendEnabled: true,
                    colorWriteMask: ColorWriteMask.All,
                    sourceColorFactor: BlendFactor.Zero,
                    destinationColorFactor: BlendFactor.SourceColor,
                    colorFunction: BlendFunction.Add,
                    sourceAlphaFactor: BlendFactor.Zero,
                    destinationAlphaFactor: BlendFactor.SourceAlpha,
                    alphaFunction: BlendFunction.Add), false),
            depthStencilState: depthStencilState,
            rasterizerState: new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            primitiveType: PrimitiveType.TriangleStrip,
            shaderSet: outputShaderSet,
            resourceLayouts: [ textPropertiesLayout, textTextureLayout ],
            output: graphicsDevice.SwapchainFramebuffer.OutputDescription
        );
        outputPipeline = factory.CreateGraphicsPipeline(outputPipelineDescription);

        var outputColorPipelineDescription = new GraphicsPipelineDescription(
            blendState: additiveBlendState,
            depthStencilState: depthStencilState,
            rasterizerState: new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            primitiveType: PrimitiveType.TriangleStrip,
            shaderSet: outputShaderSet,
            resourceLayouts: [ textPropertiesLayout, textTextureLayout ],
            output: graphicsDevice.SwapchainFramebuffer.OutputDescription
        );
        outputColorPipeline = factory.CreateGraphicsPipeline(outputColorPipelineDescription);

        var glyphPipelineDescription = new GraphicsPipelineDescription(
            blendState: additiveBlendState,
            depthStencilState: depthStencilState,
            rasterizerState: new RasterizerStateDescription(
                cullMode: FaceCullMode.None,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            primitiveType: PrimitiveType.TriangleList,
            shaderSet: new ShaderSetDescription(
                vertexLayouts:
                [
                    new VertexLayoutDescription
                    {
                        Stride = 20,
                        Elements =
                        [
                            new VertexElementDescription("Position", VertexElementFormat.Float3, 0),
                            new VertexElementDescription("Coord", VertexElementFormat.Float2, 12)
                        ]
                    }
                ],
                shader: glyphShader,
                specializations: []),
            resourceLayouts: [ textPropertiesLayout ],
            output: new OutputDescription(null,
                [new OutputAttachmentDescription(colorFormat)], TextureSampleCount.Count1)
        );
        glyphPipeline = factory.CreateGraphicsPipeline(glyphPipelineDescription);
    }

    private void CompileShaders()
    {
        glyphShader = graphicsDevice.ShaderCompiler.CompileShader(
            TextRendererShaders.GlyphTextVertex, TextRendererShaders.GlyphTextFragment, new ShaderConfig([]));

        textShader = graphicsDevice.ShaderCompiler.CompileShader(
            TextRendererShaders.TextVertex, TextRendererShaders.TextFragment, new ShaderConfig([]));
    }
}
