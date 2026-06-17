using GameEngine.Core.Content.Raw;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.Content.Processors;

public class ShaderProcessor : IContentProcessor<ShaderRaw, Shader>
{
    private readonly Engine _engine;

    public ShaderProcessor(Engine engine)
    {
        _engine = engine;
    }

    public Shader Process(ShaderRaw contentRaw)
    {
        var config = ConfigRawToConfig(contentRaw.Config);

        return _engine.GraphicsDevice.ShaderCompiler.CompileShader(
            contentRaw.VertexShader, contentRaw.FragmentShader, config);
    }

    public IContent Process(IContentRaw contentRaw)
    {
        return Process((ShaderRaw)contentRaw);
    }

    private ShaderConfig ConfigRawToConfig(ShaderConfigRaw configRaw)
    {
        var configParams = new Dictionary<string, ShaderConfigParameter>();
        foreach (var kvp in configRaw.Stages.Vertex.Parameters)
        {
            if (configParams.ContainsKey(kvp.Key))
            {
                throw new InvalidOperationException("Cannot have duplicate shader config parameter names: " + kvp.Key);
            }

            configParams.Add(kvp.Key, new ShaderConfigParameter(kvp.Value.Set, kvp.Value.Binding, kvp.Value.Offset,
                ParamTypeRawToParamType(kvp.Value.Type), ShaderConfigParameterStage.Vertex));
        }

        foreach (var kvp in configRaw.Stages.Fragment.Parameters)
        {
            if (configParams.ContainsKey(kvp.Key))
            {
                throw new InvalidOperationException("Cannot have duplicate shader config parameter names: " + kvp.Key);
            }

            configParams.Add(kvp.Key, new ShaderConfigParameter(kvp.Value.Set, kvp.Value.Binding, kvp.Value.Offset,
                ParamTypeRawToParamType(kvp.Value.Type), ShaderConfigParameterStage.Fragment));
        }

        return new ShaderConfig(configParams);
    }

    private static ShaderConfigParameterType ParamTypeRawToParamType(ShaderConfigStageParameterTypeRaw rawType)
    {
        switch (rawType)
        {
            case ShaderConfigStageParameterTypeRaw.Matrix4x4:
                return ShaderConfigParameterType.Matrix4x4;
            case ShaderConfigStageParameterTypeRaw.Float1:
                return ShaderConfigParameterType.Float1;
            case ShaderConfigStageParameterTypeRaw.Float2:
                return ShaderConfigParameterType.Float2;
            case ShaderConfigStageParameterTypeRaw.Float3:
                return ShaderConfigParameterType.Float3;
            case ShaderConfigStageParameterTypeRaw.Float4:
                return ShaderConfigParameterType.Float4;
            case ShaderConfigStageParameterTypeRaw.Texture:
                return ShaderConfigParameterType.Texture;
            case ShaderConfigStageParameterTypeRaw.Sampler:
                return ShaderConfigParameterType.Sampler;
            default:
                throw new ArgumentOutOfRangeException(nameof(rawType), rawType, "Unsupported shader config parameter type");
        }
    }
}
