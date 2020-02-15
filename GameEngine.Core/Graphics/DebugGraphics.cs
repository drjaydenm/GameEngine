using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class DebugGraphics
    {
        private readonly Engine engine;
        private readonly CommandList commandList;
        private readonly Pipeline pipeline;
        private readonly DeviceBuffer lineVertexBuffer;
        private readonly DeviceBuffer arrowVertexBuffer;
        private readonly DeviceBuffer cubeVertexBuffer;
        private readonly DeviceBuffer transformBuffer;
        private readonly DeviceBuffer colorBuffer;
        private readonly ResourceSet transformSet;

        private RgbaFloat lastColorUsed;

        public DebugGraphics(Engine engine)
        {
            this.engine = engine;
            commandList = engine.CommandList;

            var factory = engine.GraphicsDevice.ResourceFactory;

            var shader = ShaderCompiler.CompileShader(engine, ShaderCode.DebugDrawVertexCode, ShaderCode.DebugDrawFragmentCode, null);

            transformBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            colorBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<RgbaFloat>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            var transformLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WVPBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("ColorBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            transformSet = factory.CreateResourceSet(new ResourceSetDescription(
                transformLayout,
                transformBuffer,
                colorBuffer));

            var pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.None,
                fillMode: PolygonFillMode.Wireframe,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.LineList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { transformLayout };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { VertexPositionColor.VertexLayoutDescription },
                shaders: shader.Shaders);
            pipelineDescription.Outputs = engine.GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            lineVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VertexPositionColor>() * 2, BufferUsage.VertexBuffer));
            arrowVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VertexPositionColor>() * 10, BufferUsage.VertexBuffer));
            cubeVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VertexPositionColor>() * 24, BufferUsage.VertexBuffer));

            SetupCubeVertexBuffer();
        }

        public void DrawLine(Vector3 start, Vector3 end, RgbaFloat color, Matrix4x4 transform)
        {
            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, transformSet);

            SetTransformColor(transform, color);

            var vertices = new VertexPositionColor[]
            {
                new VertexPositionColor(start, color),
                new VertexPositionColor(end, color)
            };
            commandList.UpdateBuffer(lineVertexBuffer, 0, vertices);

            commandList.SetVertexBuffer(0, lineVertexBuffer);
            commandList.Draw(vertexCount: 2);
        }

        public void DrawArrow(Vector3 start, Vector3 end, RgbaFloat color, Matrix4x4 transform)
        {
            var arrowDirection = Vector3.Normalize(end - start);
            var arrowDistance = Vector3.Distance(start, end);
            var cross = Vector3.Cross(arrowDirection, Vector3.UnitY);
            var crossOfCross = Vector3.Cross(arrowDirection, cross);
            var arrowPointDistance = arrowDistance * 0.15f;
            var arrowPointSpread = arrowDistance * 0.1f;

            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, transformSet);

            SetTransformColor(transform, color);

            var vertices = new VertexPositionColor[]
            {
                // Shaft
                new VertexPositionColor(start, color),
                new VertexPositionColor(end, color),
                // Arrow pieces
                new VertexPositionColor(end, color),
                new VertexPositionColor(end - (arrowDirection * arrowPointDistance) + (cross * arrowPointSpread), color),
                new VertexPositionColor(end, color),
                new VertexPositionColor(end - (arrowDirection * arrowPointDistance) - (cross * arrowPointSpread), color),
                new VertexPositionColor(end, color),
                new VertexPositionColor(end - (arrowDirection * arrowPointDistance) + (crossOfCross * arrowPointSpread), color),
                new VertexPositionColor(end, color),
                new VertexPositionColor(end - (arrowDirection * arrowPointDistance) - (crossOfCross * arrowPointSpread), color)
            };
            commandList.UpdateBuffer(arrowVertexBuffer, 0, vertices);

            commandList.SetVertexBuffer(0, arrowVertexBuffer);
            commandList.Draw(vertexCount: 10);
        }

        public void DrawCube(RgbaFloat color, Matrix4x4 transform)
        {
            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, transformSet);

            SetTransformColor(transform, color);

            commandList.SetVertexBuffer(0, cubeVertexBuffer);
            commandList.Draw(vertexCount: 24);
        }

        private void SetupCubeVertexBuffer()
        {
            var leftVector = Vector3.UnitX * 0.5f;
            var topVector = Vector3.UnitY * 0.5f;
            var frontVector = Vector3.UnitZ * 0.5f;
            var color = RgbaFloat.White;

            var vertices = new VertexPositionColor[]
            {
                // Top
                new VertexPositionColor(topVector + frontVector + leftVector, color),
                new VertexPositionColor(topVector + frontVector + -leftVector, color),
                new VertexPositionColor(topVector + frontVector + -leftVector, color),
                new VertexPositionColor(topVector + -frontVector + -leftVector, color),
                new VertexPositionColor(topVector + -frontVector + -leftVector, color),
                new VertexPositionColor(topVector + -frontVector + leftVector, color),
                new VertexPositionColor(topVector + -frontVector + leftVector, color),
                new VertexPositionColor(topVector + frontVector + leftVector, color),
                // Bottom
                new VertexPositionColor(-topVector + frontVector + leftVector, color),
                new VertexPositionColor(-topVector + frontVector + -leftVector, color),
                new VertexPositionColor(-topVector + frontVector + -leftVector, color),
                new VertexPositionColor(-topVector + -frontVector + -leftVector, color),
                new VertexPositionColor(-topVector + -frontVector + -leftVector, color),
                new VertexPositionColor(-topVector + -frontVector + leftVector, color),
                new VertexPositionColor(-topVector + -frontVector + leftVector, color),
                new VertexPositionColor(-topVector + frontVector + leftVector, color),
                // Left
                new VertexPositionColor(topVector + -frontVector + leftVector, color),
                new VertexPositionColor(-topVector + -frontVector + leftVector, color),
                new VertexPositionColor(-topVector + frontVector + leftVector, color),
                new VertexPositionColor(topVector + frontVector + leftVector, color),
                // Right
                new VertexPositionColor(topVector + -frontVector + -leftVector, color),
                new VertexPositionColor(-topVector + -frontVector + -leftVector, color),
                new VertexPositionColor(-topVector + frontVector + -leftVector, color),
                new VertexPositionColor(topVector + frontVector + -leftVector, color)
            };
            engine.GraphicsDevice.UpdateBuffer(cubeVertexBuffer, 0, vertices);
        }

        private void SetTransformColor(Matrix4x4 transform, RgbaFloat color)
        {
            commandList.UpdateBuffer(transformBuffer, 0, transform);

            if (color != lastColorUsed)
            {
                commandList.UpdateBuffer(colorBuffer, 0, color);
                lastColorUsed = color;
            }
        }
    }
}
