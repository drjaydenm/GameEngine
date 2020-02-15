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

            for (var i = 0; i < scene.Entities.Count; i++)
            {
                for (var j = 0; j < scene.Entities[i].Components.Count; j++)
                {
                    var component = scene.Entities[i].Components[j];
                    if (!(component is IRenderable renderable))
                        continue;

                    renderable.UpdateBuffers(commandList);

                    renderable.Material.SetValue("World", renderable.WorldTransform);
                    renderable.Material.SetValue("View", camera.View);
                    renderable.Material.SetValue("Projection", camera.Projection);

                    renderable.Material.SetValue("LightDirection", LightDirection);
                    renderable.Material.SetValue("LightColor", new Vector4(0.5f, 0.5f, 0.5f, 1));
                    renderable.Material.SetValue("LightIntensity", 2);
                    renderable.Material.SetValue("AmbientLight", new Vector4(0.4f, 0.4f, 0.4f, 1));
                    renderable.Material.SetValue("FogColor", RgbaFloat.CornflowerBlue);
                    renderable.Material.SetValue("FogStartDistance", 60);
                    renderable.Material.SetValue("FogEndDistance", 150);

                    renderable.Material.SetValue("CameraDirection", camera.ViewDirection);
                    renderable.Material.SetValue("CameraPosition", camera.Position);

                    renderable.Material.Bind(commandList, this, renderable.LayoutDescription);

                    commandList.SetVertexBuffer(0, renderable.VertexBuffer);
                    commandList.SetIndexBuffer(renderable.IndexBuffer, IndexFormat.UInt32);

                    commandList.DrawIndexed(
                        indexCount: renderable.IndexBuffer.SizeInBytes / sizeof(uint),
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);
                }
            }
        }
    }
}
