using System.Numerics;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class Renderer
    {
        public DeviceBuffer ViewProjBuffer { get; private set; }
        public DeviceBuffer WorldBuffer { get; private set; }
        public DeviceBuffer CameraBuffer { get; private set; }
        public DeviceBuffer LightingBuffer { get; private set; }
        public Vector3 LightDirection { get; set; }

        private readonly Engine engine;
        private readonly Scene scene;
        private readonly CommandList commandList;

        public Renderer(Engine engine, Scene scene)
        {
            this.engine = engine;
            this.scene = scene;
            commandList = engine.CommandList;
        }

        public void Draw()
        {
            var camera = scene.ActiveCamera;
            Material lastMaterial = null;

            for (var i = 0; i < scene.Entities.Count; i++)
            {
                for (var j = 0; j < scene.Entities[i].Components.Count; j++)
                {
                    var component = scene.Entities[i].Components[j];
                    if (!(component is IRenderable renderable))
                        continue;

                    renderable.UpdateBuffers(commandList);

                    renderable.Material.SetMatrix("World", Matrix4x4.CreateTranslation(renderable.PositionOffset) * scene.Entities[i].Transform.TransformMatrix);

                    if (lastMaterial != renderable.Material)
                    {
                        renderable.Material.SetMatrix("View", camera.View);
                        renderable.Material.SetMatrix("Projection", camera.Projection);

                        renderable.Material.SetVector("LightDirection", LightDirection);
                        renderable.Material.SetVector("LightColor", new Vector4(0.5f, 0.5f, 0.5f, 1));
                        renderable.Material.SetFloat("LightIntensity", 2);
                        renderable.Material.SetVector("AmbientLight", new Vector4(0.4f, 0.4f, 0.4f, 1));
                        renderable.Material.SetVector("FogColor", RgbaFloat.CornflowerBlue.ToVector4());
                        renderable.Material.SetFloat("FogStartDistance", 60);
                        renderable.Material.SetFloat("FogEndDistance", 150);

                        renderable.Material.SetVector("SpecularColor", new Vector4(0.3f, 0.3f, 0.3f, 1));
                        renderable.Material.SetFloat("Shininess", 25);

                        renderable.Material.SetVector("CameraDirection", camera.ViewDirection);
                        renderable.Material.SetVector("CameraPosition", camera.Position);
                    }

                    renderable.Material.Bind(commandList, renderable);

                    commandList.SetVertexBuffer(0, renderable.VertexBuffer);
                    commandList.SetIndexBuffer(renderable.IndexBuffer, IndexFormat.UInt32);

                    commandList.DrawIndexed(
                        indexCount: renderable.IndexBuffer.SizeInBytes / sizeof(uint),
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);

                    lastMaterial = renderable.Material;
                }
            }
        }
    }
}
