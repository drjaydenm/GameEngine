using System.Collections.Generic;

namespace GameEngine.Core.Graphics
{
    public class ShaderConfig
    {
        public Dictionary<string, ShaderConfigParameter> Parameters { get; }

        public ShaderConfig(Dictionary<string, ShaderConfigParameter> parameters)
        {
            Parameters = parameters;
        }
    }

    public class ShaderConfigParameter
    {
        public int Set { get; }
        public int Binding { get; }
        public int Offset { get; }
        public ShaderConfigParameterType Type { get; }
        public ShaderConfigParameterStage Stage { get; }

        public ShaderConfigParameter(int set, int binding, int offset, ShaderConfigParameterType type, ShaderConfigParameterStage stage)
        {
            Set = set;
            Binding = binding;
            Offset = offset;
            Type = type;
            Stage = stage;
        }

        public override string ToString()
        {
            return $"Set: {Set} Binding: {Binding} Offset: {Offset} Type: {Type} Stage: {Stage}";
        }
    }

    public enum ShaderConfigParameterStage
    {
        Vertex,
        Fragment
    }

    public enum ShaderConfigParameterType
    {
        Matrix4x4,
        Float1,
        Float2,
        Float3,
        Float4,
        Texture,
        Sampler
    }
}
