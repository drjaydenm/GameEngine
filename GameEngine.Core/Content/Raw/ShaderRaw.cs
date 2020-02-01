namespace GameEngine.Core.Content.Raw
{
    public class ShaderRaw : IContentRaw
    {
        public string VertexShader { get; }
        public string FragmentShader { get; }

        public ShaderRaw(string vertexShader, string fragmentShader)
        {
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;
        }
    }
}
