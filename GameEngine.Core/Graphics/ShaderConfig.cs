using System.Collections.Generic;

namespace GameEngine.Core.Graphics
{
    public class ShaderConfig
    {
        public ShaderConfigStages Stages { get; }

        public ShaderConfig(ShaderConfigStage vertexStage, ShaderConfigStage fragmentStage)
        {
            Stages = new ShaderConfigStages(vertexStage, fragmentStage);
        }
    }

    public class ShaderConfigStages
    {
        public ShaderConfigStage Vertex { get; }
        public ShaderConfigStage Fragment { get; }

        public ShaderConfigStages(ShaderConfigStage vertexStage, ShaderConfigStage fragmentStage)
        {
            Vertex = vertexStage;
            Fragment = fragmentStage;
        }
    }

    public class ShaderConfigStage
    {
        public Dictionary<string, ShaderConfigStageParameter> Parameters { get; }

        public ShaderConfigStage(Dictionary<string, ShaderConfigStageParameter> parameters)
        {
            Parameters = parameters;
        }
    }

    public class ShaderConfigStageParameter
    {
        public int Set { get; }
        public int Binding { get; }
        public int Offset { get; }
        public ShaderConfigStageParameterType Type { get; }

        public ShaderConfigStageParameter(int set, int binding, int offset, ShaderConfigStageParameterType type)
        {
            Set = set;
            Binding = binding;
            Offset = offset;
            Type = type;
        }

        public override string ToString()
        {
            return $"Set: {Set} Binding: {Binding} Offset: {Offset} Type: {Type}";
        }
    }

    public enum ShaderConfigStageParameterType
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
