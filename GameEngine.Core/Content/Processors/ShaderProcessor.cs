using System;
using System.Collections.Generic;
using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.Content.Processors
{
    public class ShaderProcessor : IContentProcessor<ShaderRaw, Shader>
    {
        private readonly Engine engine;

        public ShaderProcessor(Engine engine)
        {
            this.engine = engine;
        }

        public Shader Process(ShaderRaw contentRaw)
        {
            var config = ConfigRawToConfig(contentRaw.Config);

            return ShaderCompiler.CompileShader(engine, contentRaw.VertexShader, contentRaw.FragmentShader, config);
        }

        public IContent Process(IContentRaw contentRaw)
        {
            return Process((ShaderRaw)contentRaw);
        }

        private ShaderConfig ConfigRawToConfig(ShaderConfigRaw configRaw)
        {
            var vertexParams = new Dictionary<string, ShaderConfigStageParameter>();
            foreach (var kvp in configRaw.Stages.Vertex.Parameters)
            {
                vertexParams.Add(kvp.Key, new ShaderConfigStageParameter(kvp.Value.Set, kvp.Value.Binding, kvp.Value.Offset, ParamTypeRawToParamType(kvp.Value.Type)));
            }

            var fragmentParams = new Dictionary<string, ShaderConfigStageParameter>();
            foreach (var kvp in configRaw.Stages.Fragment.Parameters)
            {
                fragmentParams.Add(kvp.Key, new ShaderConfigStageParameter(kvp.Value.Set, kvp.Value.Binding, kvp.Value.Offset, ParamTypeRawToParamType(kvp.Value.Type)));
            }

            return new ShaderConfig(new ShaderConfigStage(vertexParams), new ShaderConfigStage(fragmentParams));
        }

        private ShaderConfigStageParameterType ParamTypeRawToParamType(ShaderConfigStageParameterTypeRaw rawType)
        {
            switch (rawType)
            {
                case ShaderConfigStageParameterTypeRaw.Matrix4x4:
                    return ShaderConfigStageParameterType.Matrix4x4;
                case ShaderConfigStageParameterTypeRaw.Float1:
                    return ShaderConfigStageParameterType.Float1;
                case ShaderConfigStageParameterTypeRaw.Float2:
                    return ShaderConfigStageParameterType.Float2;
                case ShaderConfigStageParameterTypeRaw.Float3:
                    return ShaderConfigStageParameterType.Float3;
                case ShaderConfigStageParameterTypeRaw.Float4:
                    return ShaderConfigStageParameterType.Float4;
                case ShaderConfigStageParameterTypeRaw.Texture:
                    return ShaderConfigStageParameterType.Texture;
                case ShaderConfigStageParameterTypeRaw.Sampler:
                    return ShaderConfigStageParameterType.Sampler;
                default:
                    throw new Exception("Unsupported shader config parameter type");
            }
        }
    }
}
