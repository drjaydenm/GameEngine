﻿using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameEngine.Core.Graphics
{
    public class DebugGraphics
    {
        public ITexture MissingTexture { get; }

        private readonly Engine engine;
        private readonly ICommandList commandList;
        private readonly IPipeline pipeline;
        private readonly IBuffer lineVertexBuffer;
        private readonly IBuffer arrowVertexBuffer;
        private readonly IBuffer cubeVertexBuffer;
        private readonly IBuffer transformBuffer;
        private readonly IBuffer colorBuffer;
        private readonly IResourceSet transformSet;

        private Color lastColorUsed;

        public DebugGraphics(Engine engine)
        {
            this.engine = engine;
            commandList = engine.CommandList;

            var factory = engine.GraphicsDevice.ResourceFactory;

            var shader = ShaderCompiler.CompileShader(engine, ShaderCode.DebugDrawVertexCode, ShaderCode.DebugDrawFragmentCode, null);

            transformBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            colorBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<Color>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            var transformLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WVPBuffer", ResourceType.UniformBuffer, ShaderStage.Vertex),
                    new ResourceLayoutElementDescription("ColorBuffer", ResourceType.UniformBuffer, ShaderStage.Vertex)));

            transformSet = factory.CreateResourceSet(new ResourceSetDescription(transformLayout,
                [transformBuffer, colorBuffer]));

            var pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = new DepthStencilStateDescription
                {
                    DepthTestEnabled = true,
                    DepthWriteEnabled = true,
                    ComparisonType = ComparisonType.LessEqual
                },
                RasterizerState = new RasterizerStateDescription
                {
                    CullMode = FaceCullMode.None,
                    FillMode = PolygonFillMode.Wireframe,
                    FrontFace = FrontFace.Clockwise,
                    DepthClipEnabled = true,
                    ScissorTestEnabled = false
                },
                PrimitiveType = PrimitiveType.LineList,
                ShaderSet = new ShaderSetDescription
                {
                    VertexLayouts = [VertexPositionColor.VertexLayoutDescription],
                    Shader = shader
                },
                ResourceLayouts = [transformLayout],
                Output = engine.GraphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            lineVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VertexPositionColor>() * 2, BufferUsage.VertexBuffer));
            arrowVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VertexPositionColor>() * 10, BufferUsage.VertexBuffer));
            cubeVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<VertexPositionColor>() * 24, BufferUsage.VertexBuffer));

            SetupCubeVertexBuffer();

            MissingTexture = factory.CreateTexture(new global::Veldrid.TextureDescription(2, 2, 1, 1, 1, global::Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, global::Veldrid.TextureUsage.Sampled, global::Veldrid.TextureType.Texture2D));
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, Matrix4x4 transform)
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
            commandList.Draw(vertexCount: 2, instanceCount: 1, vertexStart: 0, instanceStart: 0);
        }

        public void DrawArrow(Vector3 start, Vector3 end, Color color, Matrix4x4 transform)
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
            commandList.Draw(vertexCount: 10, instanceCount: 1, vertexStart: 0, instanceStart: 0);
        }

        public void DrawCube(Color color, Matrix4x4 transform)
        {
            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, transformSet);

            SetTransformColor(transform, color);

            commandList.SetVertexBuffer(0, cubeVertexBuffer);
            commandList.Draw(vertexCount: 24, instanceCount: 1, vertexStart: 0, instanceStart: 0);
        }

        private void SetupCubeVertexBuffer()
        {
            var leftVector = Vector3.UnitX * 0.5f;
            var topVector = Vector3.UnitY * 0.5f;
            var frontVector = Vector3.UnitZ * 0.5f;
            var color = Color.White;

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

        private void SetTransformColor(Matrix4x4 transform, Color color)
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
