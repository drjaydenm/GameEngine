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

            SetDefaultParameterValues();
            InitBuffersResourceSets();
            CreateBuffersResourceSets();
        }

        public void SetValue<T>(string name, T value) where T : struct
        {
            if (!(value is float) && !(value is Vector2) && !(value is Vector3) && !(value is Vector4) && !(value is Matrix4x4))
                return;

            if (!shader.Config.Parameters.TryGetValue(name, out var param))
                return;

            if (!IsValueType(param.Type))
                return;

            parameterValues[name] = value;

            var buffer = buffers[(param.Set, param.Binding)];
            engine.GraphicsDevice.UpdateBuffer(buffer, (uint)param.Offset, value);
        }

        public void SetTexture(string name, Texture value)
        {
            if (!shader.Config.Parameters.TryGetValue(name, out var param))
                return;

            if (param.Type != ShaderConfigParameterType.Texture)
                return;

            parameterValues[name] = value;

            var resourceSet = resourceSets[param.Set];
            resourceSet.Dispose();

            CreateResourceSet(param.Set);
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
            var factory = engine.GraphicsDevice.ResourceFactory;

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
            pipelineDescription.ResourceLayouts = resourceLayouts.Values.ToArray();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: shader.Shaders);
            pipelineDescription.Outputs = engine.GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            mustSetup = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDefaultParameterValues()
        {
            foreach (var kvp in shader.Config.Parameters)
            {
                switch (kvp.Value.Type)
                {
                    case ShaderConfigParameterType.Float1:
                        parameterValues.Add(kvp.Key, 0);
                        break;
                    case ShaderConfigParameterType.Float2:
                        parameterValues.Add(kvp.Key, Vector2.Zero);
                        break;
                    case ShaderConfigParameterType.Float3:
                        parameterValues.Add(kvp.Key, Vector3.Zero);
                        break;
                    case ShaderConfigParameterType.Float4:
                        parameterValues.Add(kvp.Key, Vector4.Zero);
                        break;
                    case ShaderConfigParameterType.Matrix4x4:
                        parameterValues.Add(kvp.Key, Matrix4x4.Identity);
                        break;
                    case ShaderConfigParameterType.Sampler:
                        parameterValues.Add(kvp.Key, engine.GraphicsDevice.LinearSampler);
                        break;
                    case ShaderConfigParameterType.Texture:
                        parameterValues.Add(kvp.Key, engine.DebugGraphics.MissingTexture);
                        break;
                    default:
                        throw new Exception("Unknown shader config parameter type");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitBuffersResourceSets()
        {
            foreach (var kvp in shader.Config.Parameters)
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

                foreach (var kvp2 in shader.Config.Parameters)
                {
                    if (setBindingIndex.Item1 == kvp2.Value.Set && setBindingIndex.Item2 == kvp2.Value.Binding)
                    {
                        // Default to 16 bytes as smaller elements need to be padded up anyway
                        bufferSize += (kvp2.Value.Type == ShaderConfigParameterType.Matrix4x4 ? 64U : 16U);
                    }
                }

                var buffer = engine.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.UniformBuffer));

                buffers[setBindingIndex] = buffer;
            }

            // Create the resource layouts
            foreach (var setIndex in resourceLayouts.Keys.ToList())
            {
                var layoutElements = new Dictionary<int, ResourceLayoutElementDescription>();

                foreach (var kvp2 in shader.Config.Parameters)
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
                CreateResourceSet(setIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateResourceSet(int setIndex)
        {
            var resources = new Dictionary<int, BindableResource>();

            foreach (var kvp2 in shader.Config.Parameters)
            {
                if (setIndex == kvp2.Value.Set && !resources.ContainsKey(kvp2.Value.Binding))
                {
                    if (IsValueType(kvp2.Value.Type))
                    {
                        resources.Add(kvp2.Value.Binding, buffers[(setIndex, kvp2.Value.Binding)]);
                    }
                    else
                    {
                        if (kvp2.Value.Type == ShaderConfigParameterType.Sampler)
                        {
                            resources.Add(kvp2.Value.Binding, (Sampler)parameterValues[kvp2.Key]);
                        }
                        else if (kvp2.Value.Type == ShaderConfigParameterType.Texture)
                        {
                            resources.Add(kvp2.Value.Binding, ((Texture)parameterValues[kvp2.Key]).NativeTexture);
                        }
                    }
                }
            }

            var resourceSet = engine.GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(resourceLayouts[setIndex], resources.Values.ToArray()));

            resourceSets[setIndex] = resourceSet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValueType(ShaderConfigParameterType type)
        {
            return type == ShaderConfigParameterType.Float1
                || type == ShaderConfigParameterType.Float2
                || type == ShaderConfigParameterType.Float3
                || type == ShaderConfigParameterType.Float4
                || type == ShaderConfigParameterType.Matrix4x4;
        }

        private ResourceKind ShaderParamTypeToResourceKind(ShaderConfigParameterType type)
        {
            switch (type)
            {
                case ShaderConfigParameterType.Float1:
                case ShaderConfigParameterType.Float2:
                case ShaderConfigParameterType.Float3:
                case ShaderConfigParameterType.Float4:
                case ShaderConfigParameterType.Matrix4x4:
                    return ResourceKind.UniformBuffer;
                case ShaderConfigParameterType.Texture:
                    return ResourceKind.TextureReadOnly;
                case ShaderConfigParameterType.Sampler:
                    return ResourceKind.Sampler;
                default:
                    throw new Exception("Unknown shader config param type");
            }
        }
    }
}
