namespace GameEngine.Core.Content.Raw
{
    public class ShaderRaw : IContentRaw
    {
        public string VertexShader { get; }
        public string FragmentShader { get; }
        public ShaderConfigRaw Config { get; }

        public ShaderRaw(string vertexShader, string fragmentShader, ShaderConfigRaw config)
        {
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;
            Config = config;
        }
    }
}
