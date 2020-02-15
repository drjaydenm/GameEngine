using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class Material
    {
        private readonly Engine engine;
        private readonly Shader shader;

        private Dictionary<(int, int), DeviceBuffer> buffers;
        private Dictionary<int, ResourceLayout> resourceLayouts;
        private Dictionary<int, ResourceSet> resourceSets;
        private Dictionary<string, object> parameterValues;
        private bool mustSetup = true;
        private Pipeline pipeline;

        public Material(Engine engine, Shader shader, Texture t)
        {
            this.engine = engine;
            this.shader = shader;

            buffers = new Dictionary<(int, int), DeviceBuffer>();
            resourceLayouts = new Dictionary<int, ResourceLayout>();
            resourceSets = new Dictionary<int, ResourceSet>();
            parameterValues = new Dictionary<string, object>();

            InitBuffersResourceSets(shader.Config.Stages.Vertex);
            InitBuffersResourceSets(shader.Config.Stages.Fragment);

            CreateBuffersResourceSets();
        }

        public void SetValue<T>(string name, T value) where T : struct
        {
            if (!(value is float) && !(value is Vector2) && !(value is Vector3) && !(value is Vector4) && !(value is Matrix4x4))
                return;

            if (!shader.Config.Stages.Vertex.Parameters.TryGetValue(name, out var param)
                && !shader.Config.Stages.Fragment.Parameters.TryGetValue(name, out param))
                return;

            if (!IsValueType(param.Type))
                return;

            var buffer = buffers[(param.Set, param.Binding)];
            engine.GraphicsDevice.UpdateBuffer(buffer, (uint)param.Offset, value);
        }

        public void SetTexture(string name, Texture value)
        {
            if (!shader.Config.Stages.Vertex.Parameters.TryGetValue(name, out var param)
                && !shader.Config.Stages.Fragment.Parameters.TryGetValue(name, out param))
                return;

            if (param.Type != ShaderConfigStageParameterType.Texture)
                return;

            var resourceSet = resourceSets[param.Set];
            resourceSet.Dispose();

            // TODO create a new resource set
        }

        internal void Bind(CommandList commandList, Renderer renderer, VertexLayoutDescription vertexLayout)
        {
            if (mustSetup)
                Setup(renderer, vertexLayout);

            commandList.SetPipeline(pipeline);
            foreach (var kvp in resourceSets)
            {
                commandList.SetGraphicsResourceSet((uint)kvp.Key, kvp.Value);
            }
        }

        private void Setup(Renderer renderer, VertexLayoutDescription vertexLayout)
        {
            /*var factory = engine.GraphicsDevice.ResourceFactory;

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
                engine.GraphicsDevice.PointSampler));

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

            mustSetup = false;*/
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitBuffersResourceSets(ShaderConfigStage stage)
        {
            foreach (var kvp in stage.Parameters)
            {
                var setBindingIndex = (kvp.Value.Set, kvp.Value.Binding);
                if (!buffers.ContainsKey(setBindingIndex) && IsValueType(kvp.Value.Type))
                {
                    buffers.Add(setBindingIndex, null);
                }

                if (!resourceLayouts.ContainsKey(kvp.Value.Set))
                    resourceLayouts.Add(kvp.Value.Set, null);

                if (!resourceSets.ContainsKey(kvp.Value.Set))
                    resourceSets.Add(kvp.Value.Set, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateBuffersResourceSets()
        {
            // Create the buffers
            foreach (var setBindingIndex in buffers.Keys.ToList())
            {
                uint bufferSize = 0;

                foreach (var kvp2 in shader.Config.Stages.Vertex.Parameters)
                {
                    if (setBindingIndex.Item1 == kvp2.Value.Set && setBindingIndex.Item2 == kvp2.Value.Binding)
                    {
                        // Default to 16 bytes as smaller elements need to be padded up anyway
                        bufferSize += (kvp2.Value.Type == ShaderConfigStageParameterType.Matrix4x4 ? 64U : 16U);
                    }
                }

                foreach (var kvp2 in shader.Config.Stages.Fragment.Parameters)
                {
                    if (setBindingIndex.Item1 == kvp2.Value.Set && setBindingIndex.Item2 == kvp2.Value.Binding)
                    {
                        // Default to 16 bytes as smaller elements need to be padded up anyway
                        bufferSize += (kvp2.Value.Type == ShaderConfigStageParameterType.Matrix4x4 ? 64U : 16U);
                    }
                }

                var buffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.UniformBuffer));

                buffers[setBindingIndex] = buffer;
            }

            // Create the resource layouts
            foreach (var setIndex in resourceLayouts.Keys.ToList())
            {
                var layoutElements = new Dictionary<int, ResourceLayoutElementDescription>();

                foreach (var kvp2 in shader.Config.Stages.Vertex.Parameters)
                {
                    if (setIndex == kvp2.Value.Set && !layoutElements.ContainsKey(kvp2.Value.Binding))
                    {
                        var resourceType = ShaderParamTypeToResourceKind(kvp2.Value.Type);
                        layoutElements.Add(kvp2.Value.Binding,
                            new ResourceLayoutElementDescription($"VertLayout_{kvp2.Value.Set}_{kvp2.Value.Binding}", resourceType, ShaderStages.Vertex));
                    }
                }

                foreach (var kvp2 in shader.Config.Stages.Fragment.Parameters)
                {
                    if (setIndex == kvp2.Value.Set && !layoutElements.ContainsKey(kvp2.Value.Binding))
                    {
                        var resourceType = ShaderParamTypeToResourceKind(kvp2.Value.Type);
                        layoutElements.Add(kvp2.Value.Binding,
                            new ResourceLayoutElementDescription($"FragLayout_{kvp2.Value.Set}_{kvp2.Value.Binding}", resourceType, ShaderStages.Fragment));
                    }
                }

                var layoutDesc = new ResourceLayoutDescription(layoutElements.Values.ToArray());
                var resourceLayout = engine.GraphicsDevice.ResourceFactory.CreateResourceLayout(layoutDesc);

                resourceLayouts[setIndex] = resourceLayout;
            }

            // Create the resource sets
            foreach (var setIndex in resourceSets.Keys.ToList())
            {
                var resources = new Dictionary<int, BindableResource>();

                foreach (var kvp2 in shader.Config.Stages.Vertex.Parameters)
                {
                    if (setIndex == kvp2.Value.Set && !resources.ContainsKey(kvp2.Value.Binding))
                    {
                        if (IsValueType(kvp2.Value.Type))
                        {
                            resources.Add(kvp2.Value.Binding, buffers[(setIndex, kvp2.Value.Binding)]);
                        }
                        else
                        {
                            if (kvp2.Value.Type == ShaderConfigStageParameterType.Sampler)
                            {
                                resources.Add(kvp2.Value.Binding, engine.GraphicsDevice.LinearSampler);
                            }
                            else if (kvp2.Value.Type == ShaderConfigStageParameterType.Texture)
                            {
                                resources.Add(kvp2.Value.Binding, engine.DebugGraphics.MissingTexture.NativeTexture);
                            }
                        }
                    }
                }

                foreach (var kvp2 in shader.Config.Stages.Fragment.Parameters)
                {
                    if (setIndex == kvp2.Value.Set && !resources.ContainsKey(kvp2.Value.Binding))
                    {
                        if (IsValueType(kvp2.Value.Type))
                        {
                            resources.Add(kvp2.Value.Binding, buffers[(setIndex, kvp2.Value.Binding)]);
                        }
                        else
                        {
                            if (kvp2.Value.Type == ShaderConfigStageParameterType.Sampler)
                            {
                                resources.Add(kvp2.Value.Binding, engine.GraphicsDevice.LinearSampler);
                            }
                            else if (kvp2.Value.Type == ShaderConfigStageParameterType.Texture)
                            {
                                resources.Add(kvp2.Value.Binding, engine.DebugGraphics.MissingTexture.NativeTexture);
                            }
                        }
                    }
                }

                var resourceSet = engine.GraphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(resourceLayouts[setIndex], resources.Values.ToArray()));

                resourceSets[setIndex] = resourceSet;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValueType(ShaderConfigStageParameterType type)
        {
            return type == ShaderConfigStageParameterType.Float1
                || type == ShaderConfigStageParameterType.Float2
                || type == ShaderConfigStageParameterType.Float3
                || type == ShaderConfigStageParameterType.Float4
                || type == ShaderConfigStageParameterType.Matrix4x4;
        }

        private ResourceKind ShaderParamTypeToResourceKind(ShaderConfigStageParameterType type)
        {
            switch (type)
            {
                case ShaderConfigStageParameterType.Float1:
                case ShaderConfigStageParameterType.Float2:
                case ShaderConfigStageParameterType.Float3:
                case ShaderConfigStageParameterType.Float4:
                case ShaderConfigStageParameterType.Matrix4x4:
                    return ResourceKind.UniformBuffer;
                case ShaderConfigStageParameterType.Texture:
                    return ResourceKind.TextureReadOnly;
                case ShaderConfigStageParameterType.Sampler:
                    return ResourceKind.Sampler;
                default:
                    throw new Exception("Unknown shader config param type");
            }
        }
    }
}
