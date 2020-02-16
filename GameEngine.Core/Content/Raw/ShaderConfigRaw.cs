using System.Collections.Generic;

namespace GameEngine.Core.Content.Raw
{
    public class ShaderConfigRaw
    {
        public ShaderConfigStagesRaw Stages { get; set; }
    }

    public class ShaderConfigStagesRaw
    {
        public ShaderConfigStageRaw Vertex { get; set; }
        public ShaderConfigStageRaw Fragment { get; set; }
    }

    public class ShaderConfigStageRaw
    {
        public Dictionary<string, ShaderConfigStageParameterRaw> Parameters { get; set; }
    }

    public class ShaderConfigStageParameterRaw
    {
        public int Set { get; set; }
        public int Binding { get; set; }
        public int Offset { get; set; }
        public ShaderConfigStageParameterTypeRaw Type { get; set; }

        public override string ToString()
        {
            return $"Set: {Set} Binding: {Binding} Offset: {Offset} Type: {Type}";
        }
    }

    public enum ShaderConfigStageParameterTypeRaw
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
