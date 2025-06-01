using GameEngine.Core.Content;
using GameEngine.Core.Graphics.Veldrid;
using Veldrid.SPIRV;

namespace GameEngine.Core.Graphics
{
    /// <summary>
    /// Contains data for a Shader. Use the <c>ShaderCompiler</c> to create an instance
    /// </summary>
    public class Shader : IContent
    {
        internal global::Veldrid.Shader[] Shaders { get; }
        internal VertexElementDescription[] VertexElements { get; }
        internal ResourceLayoutDescription[] ResourceLayouts { get; }
        internal ShaderConfig Config { get; }

        internal Shader(global::Veldrid.Shader[] shaders, SpirvReflection reflection, ShaderConfig config)
        {
            Shaders = shaders;
            Config = config;

            if (reflection != null)
            {
                var elements = new VertexElementDescription[reflection.VertexElements.Length];
                for (var i = 0; i < reflection.VertexElements.Length; i++)
                {
                    var element = reflection.VertexElements[i];
                    elements[i] = new VertexElementDescription(element.Name,
                        VeldridUtils.ConvertVertexFormatFromVeldrid(element.Format), element.Offset);
                }

                var layouts = new ResourceLayoutDescription[reflection.ResourceLayouts.Length];
                for (var i = 0; i < reflection.ResourceLayouts.Length; i++)
                {
                    var resourceElements = new ResourceLayoutElementDescription[reflection.ResourceLayouts[i].Elements.Length];
                    for (var j = 0; j < reflection.ResourceLayouts[i].Elements.Length; j++)
                    {
                        var element = reflection.ResourceLayouts[i].Elements[j];
                        resourceElements[j] = new ResourceLayoutElementDescription(
                            element.Name, (ResourceType)element.Kind, (ShaderStage)element.Stages);
                    }

                    layouts[i] = new ResourceLayoutDescription(resourceElements);
                }

                VertexElements = elements;
                ResourceLayouts = layouts;
            }
            else
            {
                VertexElements = new VertexElementDescription[0];
                ResourceLayouts = new ResourceLayoutDescription[0];
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < Shaders.Length; i++)
            {
                Shaders[i].Dispose();
            }
        }
    }
}
