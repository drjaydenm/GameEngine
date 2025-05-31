using System.Numerics;
using System.Runtime.CompilerServices;
using GameEngine.Core.Graphics.Veldrid;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class Material
    {
        private readonly Engine engine;
        private readonly Shader shader;

        private Dictionary<(int, int), IBuffer> buffers;
        private Dictionary<int, ResourceLayout> resourceLayouts;
        private Dictionary<int, ResourceSet> resourceSets;
        private Dictionary<string, object> parameterValues;
        private Queue<string> dirtyParameters;
        private bool mustSetup = true;
        private Pipeline pipeline;

        public Material(Engine engine, Shader shader)
        {
            this.engine = engine;
            this.shader = shader;

            buffers = new Dictionary<(int, int), IBuffer>();
            resourceLayouts = new Dictionary<int, ResourceLayout>();
            resourceSets = new Dictionary<int, ResourceSet>();
            parameterValues = new Dictionary<string, object>();
            dirtyParameters = new Queue<string>();

            SetDefaultParameterValues();
            InitBuffersResourceSets();
            CreateBuffersResourceSets();
        }

        public void SetFloat(string name, float value)
        {
            if (shader.Config.Parameters[name].Type != ShaderConfigParameterType.Float1)
                throw new Exception("The specified parameter is not a float type: " + name);

            if ((float)parameterValues[name] == value)
                return;

            parameterValues[name] = value;
            dirtyParameters.Enqueue(name);
        }

        public void SetMatrix(string name, Matrix4x4 value)
        {
            if (shader.Config.Parameters[name].Type != ShaderConfigParameterType.Matrix4x4)
                throw new Exception("The specified parameter is not a matrix type: " + name);

            if ((Matrix4x4)parameterValues[name] == value)
                return;

            parameterValues[name] = value;
            dirtyParameters.Enqueue(name);
        }

        public void SetTexture(string name, Texture value)
        {
            if (!shader.Config.Parameters.TryGetValue(name, out var param))
                throw new Exception("Cannot find parameter in shader: " + name);

            if (param.Type != ShaderConfigParameterType.Texture)
                throw new Exception("The specified parameter is not a texture type: " + name);

            if ((Texture)parameterValues[name] == value)
                return;

            parameterValues[name] = value;

            var resourceSet = resourceSets[param.Set];
            resourceSet.Dispose();

            CreateResourceSet(param.Set);
        }

        public void SetSampler(string name, Sampler value)
        {
            if (!shader.Config.Parameters.TryGetValue(name, out var param))
                throw new Exception("Cannot find parameter in shader: " + name);

            if (param.Type != ShaderConfigParameterType.Sampler)
                throw new Exception("The specified parameter is not a sampler type: " + name);

            if ((Sampler)parameterValues[name] == value)
                return;

            parameterValues[name] = value;

            var resourceSet = resourceSets[param.Set];
            resourceSet.Dispose();

            CreateResourceSet(param.Set);
        }

        public void SetVector(string name, Vector2 value)
        {
            if (shader.Config.Parameters[name].Type != ShaderConfigParameterType.Float2)
                throw new Exception("The specified parameter is not a Vector2 type: " + name);

            SetVectorImpl(name, new Vector4(value, 0, 0));
        }

        public void SetVector(string name, Vector3 value)
        {
            if (shader.Config.Parameters[name].Type != ShaderConfigParameterType.Float3)
                throw new Exception("The specified parameter is not a Vector3 type: " + name);

            SetVectorImpl(name, new Vector4(value, 0));
        }

        public void SetVector(string name, Vector4 value)
        {
            if (shader.Config.Parameters[name].Type != ShaderConfigParameterType.Float4)
                throw new Exception("The specified parameter is not a Vector4 type: " + name);

            SetVectorImpl(name, value);
        }

        internal void Bind(ICommandList commandList, PrimitiveType primitiveType, VertexLayoutDescription layoutDescription, FaceCullMode cullMode)
        {
            if (mustSetup)
                Setup(primitiveType, layoutDescription, cullMode);

            while (dirtyParameters.Count > 0)
            {
                var paramKey = dirtyParameters.Dequeue();
                var param = shader.Config.Parameters[paramKey];
                var buffer = buffers[(param.Set, param.Binding)];

                switch (param.Type)
                {
                    case ShaderConfigParameterType.Float1:
                        commandList.UpdateBuffer(buffer, (uint)param.Offset, (float)parameterValues[paramKey]);
                        break;
                    case ShaderConfigParameterType.Float2:
                    case ShaderConfigParameterType.Float3:
                    case ShaderConfigParameterType.Float4:
                        commandList.UpdateBuffer(buffer, (uint)param.Offset, (Vector4)parameterValues[paramKey]);
                        break;
                    case ShaderConfigParameterType.Matrix4x4:
                        commandList.UpdateBuffer(buffer, (uint)param.Offset, (Matrix4x4)parameterValues[paramKey]);
                        break;
                    default:
                        throw new Exception("Unhandled shader config parameter type");
                }
            }

            commandList.SetPipeline(pipeline);
            foreach (var kvp in resourceSets)
            {
                commandList.SetGraphicsResourceSet((uint)kvp.Key, kvp.Value);
            }
        }

        private void Setup(PrimitiveType primitiveType, VertexLayoutDescription layoutDescription, FaceCullMode cullMode)
        {
            var factory = engine.GraphicsDevice.ResourceFactory;

            var pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: cullMode,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTypeToTopology(primitiveType);
            pipelineDescription.ResourceLayouts = resourceLayouts.Values.ToArray();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { layoutDescription },
                shaders: shader.Shaders);
            pipelineDescription.Outputs = engine.GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            mustSetup = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PrimitiveTopology PrimitiveTypeToTopology(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.TriangleList:
                    return PrimitiveTopology.TriangleList;
                case PrimitiveType.TriangleStrip:
                    return PrimitiveTopology.TriangleStrip;
                case PrimitiveType.LineList:
                    return PrimitiveTopology.LineList;
                case PrimitiveType.LineStrip:
                    return PrimitiveTopology.LineStrip;
                default:
                    throw new Exception("Unknown primitive type");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDefaultParameterValues()
        {
            foreach (var kvp in shader.Config.Parameters)
            {
                switch (kvp.Value.Type)
                {
                    case ShaderConfigParameterType.Float1:
                        parameterValues.Add(kvp.Key, 0f);
                        break;
                    case ShaderConfigParameterType.Float2:
                    case ShaderConfigParameterType.Float3:
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
                        var resourceLayoutName = GetResourceLayoutName(kvp2.Value.Set, kvp2.Value.Binding, kvp2.Value.Stage);
                        var resourceType = ShaderParamTypeToResourceKind(kvp2.Value.Type);
                        var shaderStage = kvp2.Value.Stage == ShaderConfigParameterStage.Vertex ? ShaderStages.Vertex : ShaderStages.Fragment;
                        layoutElements.Add(kvp2.Value.Binding,
                            new ResourceLayoutElementDescription(resourceLayoutName, resourceType, shaderStage));
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
            // HACK change this to use IBindableResource once all resource types are abstracted
            var resources = new Dictionary<int, BindableResource>();

            foreach (var kvp2 in shader.Config.Parameters)
            {
                if (setIndex == kvp2.Value.Set && !resources.ContainsKey(kvp2.Value.Binding))
                {
                    if (IsValueType(kvp2.Value.Type))
                    {
                        // HACK change this to use IBindableResource once all resource types are abstracted
                        resources.Add(kvp2.Value.Binding, ((VeldridBuffer)buffers[(setIndex, kvp2.Value.Binding)]).UnderlyingBuffer);
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
        private void SetVectorImpl(string name, Vector4 value)
        {
            if ((Vector4)parameterValues[name] == value)
                return;

            parameterValues[name] = value;
            dirtyParameters.Enqueue(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetResourceLayoutName(int setIndex, int bindingIndex, ShaderConfigParameterStage stage)
        {
            if (shader.ResourceLayouts.Length > setIndex && shader.ResourceLayouts[setIndex].Elements.Length > bindingIndex)
            {
                return shader.ResourceLayouts[setIndex].Elements[bindingIndex].Name;
            }

            return $"{stage}Layout_{setIndex}_{bindingIndex}";
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
