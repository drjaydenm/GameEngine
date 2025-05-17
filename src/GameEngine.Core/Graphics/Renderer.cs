using System.Numerics;
using Veldrid;

namespace GameEngine.Core.Graphics
{
    public class Renderer
    {
        public Vector3 LightDirection { get; set; } = Vector3.Normalize(new Vector3(1, -1, 1));
        public Vector4 LightColor { get; set; } = new Vector4(0.5f, 0.5f, 0.5f, 1);
        public float LightIntensity { get; set; } = 2f;
        public Vector4 AmbientLight { get; set; } = new Vector4(0.4f, 0.4f, 0.4f, 1);
        public Vector4 FogColor { get; set; } = Color.CornflowerBlue.ToVector4();
        public float FogStartDistance = 200;
        public float FogEndDistance = 400;
        public Material SkyboxMaterial { get; set; }

        private readonly Engine engine;
        private readonly Scene scene;
        private readonly CommandList commandList;
        private DeviceBuffer skyboxVertices;
        private DeviceBuffer skyboxIndices;

        public Renderer(Engine engine, Scene scene)
        {
            this.engine = engine;
            this.scene = scene;
            commandList = engine.CommandList;

            SetupSkybox();
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
                        renderable.Material.SetVector("LightColor", LightColor);
                        renderable.Material.SetFloat("LightIntensity", LightIntensity);
                        renderable.Material.SetVector("AmbientLight", AmbientLight);
                        renderable.Material.SetVector("FogColor", FogColor);
                        renderable.Material.SetFloat("FogStartDistance", FogStartDistance);
                        renderable.Material.SetFloat("FogEndDistance", FogEndDistance);

                        // TODO move to the material
                        renderable.Material.SetVector("SpecularColor", new Vector4(0.3f, 0.3f, 0.3f, 1));
                        renderable.Material.SetFloat("Shininess", 25);

                        renderable.Material.SetVector("CameraDirection", camera.ViewDirection);
                        renderable.Material.SetVector("CameraPosition", camera.Position);
                    }

                    renderable.Material.Bind(commandList, renderable.PrimitiveType, renderable.LayoutDescription, FaceCullMode.Back);

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

            DrawSkybox();
        }

        private void SetupSkybox()
        {
            var vertices = ShapeBuilder.BuildCubeVertices();
            var indices = ShapeBuilder.BuildCubeIndicies();
            var factory = engine.GraphicsDevice.ResourceFactory;
            skyboxVertices = factory.CreateBuffer(new BufferDescription(VertexPositionNormalTexCoord.SizeInBytes * (uint)vertices.Length, BufferUsage.VertexBuffer));
            skyboxIndices = factory.CreateBuffer(new BufferDescription(sizeof(uint) * (uint)indices.Length, BufferUsage.IndexBuffer));
            engine.GraphicsDevice.UpdateBuffer(skyboxVertices, 0, vertices);
            engine.GraphicsDevice.UpdateBuffer(skyboxIndices, 0, indices);

            var shaderConfig = new ShaderConfig(new Dictionary<string, ShaderConfigParameter>()
            {
                { "View", new ShaderConfigParameter(0, 0, 0, ShaderConfigParameterType.Matrix4x4, ShaderConfigParameterStage.Vertex) },
                { "Projection", new ShaderConfigParameter(0, 0, 64, ShaderConfigParameterType.Matrix4x4, ShaderConfigParameterStage.Vertex) },
                { "SkyColor", new ShaderConfigParameter(1, 0, 0, ShaderConfigParameterType.Float4, ShaderConfigParameterStage.Fragment) },
                { "SunColor", new ShaderConfigParameter(1, 0, 16, ShaderConfigParameterType.Float4, ShaderConfigParameterStage.Fragment) },
                { "HorizonColor", new ShaderConfigParameter(1, 0, 32, ShaderConfigParameterType.Float4, ShaderConfigParameterStage.Fragment) },
                { "BottomColor", new ShaderConfigParameter(1, 0, 48, ShaderConfigParameterType.Float4, ShaderConfigParameterStage.Fragment) },
                { "SunDirection", new ShaderConfigParameter(1, 0, 64, ShaderConfigParameterType.Float3, ShaderConfigParameterStage.Fragment) }
            });
            var shader = ShaderCompiler.CompileShader(engine, ShaderCode.SkyboxVertexCode, ShaderCode.SkyboxFragmentCode, shaderConfig);
            SkyboxMaterial = new Material(engine, shader);

            SkyboxMaterial.SetVector("SkyColor", new Vector4(0.26f, 0.45f, 0.66f, 1));
            SkyboxMaterial.SetVector("SunColor", new Vector4(1, 1, 0.9f, 1));
            SkyboxMaterial.SetVector("HorizonColor", new Vector4(0.51f, 0.74f, 0.92f, 1));
            SkyboxMaterial.SetVector("BottomColor", new Vector4(0.4f, 0.4f, 0.4f, 1));
        }

        private void DrawSkybox()
        {
            var camera = scene.ActiveCamera;

            // Clear out translation from the view matrix to center the skybox around the camera
            var skyboxView = camera.View;
            skyboxView.M14 = 0;
            skyboxView.M24 = 0;
            skyboxView.M34 = 0;
            skyboxView.M41 = 0;
            skyboxView.M42 = 0;
            skyboxView.M43 = 0;
            skyboxView.M44 = 1;

            SkyboxMaterial.SetMatrix("View", skyboxView);
            SkyboxMaterial.SetMatrix("Projection", camera.Projection);
            SkyboxMaterial.SetVector("SunDirection", LightDirection);
            SkyboxMaterial.Bind(commandList, PrimitiveType.TriangleList, VertexPositionNormalTexCoord.VertexLayoutDescription, FaceCullMode.None);

            commandList.SetVertexBuffer(0, skyboxVertices);
            commandList.SetIndexBuffer(skyboxIndices, IndexFormat.UInt32);

            commandList.DrawIndexed(
                indexCount: skyboxIndices.SizeInBytes / sizeof(uint),
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }
    }
}
