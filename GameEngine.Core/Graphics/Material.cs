using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class Material
    {
        private readonly Engine engine;
        private readonly Shader shader;

        private bool mustSetup = true;
        private ResourceSet transformSet;
        private ResourceSet sceneSet;
        private ResourceSet materialSet;
        private ResourceSet textureSet;
        private Pipeline pipeline;
        private DeviceBuffer materialBuffer;
        private MaterialInfo materialInfo;
        private Texture texture;

        public Material(Engine engine, Shader shader, Texture texture)
        {
            this.engine = engine;
            this.shader = shader;
            this.texture = texture;
        }

        public void Bind(CommandList commandList, Renderer renderer, VertexLayoutDescription vertexLayout)
        {
            if (mustSetup)
                Setup(renderer, vertexLayout);

            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, transformSet);
            commandList.SetGraphicsResourceSet(1, sceneSet);
            commandList.SetGraphicsResourceSet(2, materialSet);
            commandList.SetGraphicsResourceSet(3, textureSet);
        }

        private void Setup(Renderer renderer, VertexLayoutDescription vertexLayout)
        {
            var factory = engine.GraphicsDevice.ResourceFactory;

            materialBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<MaterialInfo>(), BufferUsage.UniformBuffer));
            // TODO these should be passed in
            materialInfo.SpecularColor = new RgbaFloat(0.3f, 0.3f, 0.3f, 1);
            materialInfo.Shininess = 100;
            engine.GraphicsDevice.UpdateBuffer(materialBuffer, 0, materialInfo);

            var transformLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewProjBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            var sceneLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("CameraBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("LightingBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            var materialLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MaterialBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment)));

            var textureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("TextureSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            transformSet = factory.CreateResourceSet(new ResourceSetDescription(
                transformLayout,
                renderer.ViewProjBuffer,
                renderer.WorldBuffer));

            sceneSet = factory.CreateResourceSet(new ResourceSetDescription(
                sceneLayout,
                renderer.CameraBuffer,
                renderer.LightingBuffer));

            materialSet = factory.CreateResourceSet(new ResourceSetDescription(
                materialLayout,
                materialBuffer));

            textureSet = factory.CreateResourceSet(new ResourceSetDescription(
                textureLayout,
                texture.NativeTexture,
                engine.GraphicsDevice.LinearSampler));

            var pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { transformLayout, sceneLayout, materialLayout, textureLayout };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: shader.Shaders);
            pipelineDescription.Outputs = engine.GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            mustSetup = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MaterialInfo
        {
            public RgbaFloat SpecularColor;
            public float Shininess;
            private float _padding1;
            private float _padding2;
            private float _padding3;
        }
    }
}
