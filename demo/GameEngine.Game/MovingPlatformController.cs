using System.Numerics;
using GameEngine.Core;
using GameEngine.Core.Entities;
using GameEngine.Core.Graphics;

namespace GameEngine.Game
{
    public class MovingPlatformController : IComponent
    {
        public Vector3 MovementDirection = Vector3.UnitY;
        public float MovementSpeed = 1f;
        public Vector3 PlatformSize = new Vector3(1, 0.1f, 1);

        private readonly Engine engine;
        private Entity entity;
        private BasicRenderable<VertexPositionNormalTexCoordMaterial> renderable;
        private Material material;
        private PhysicsBoxComponent physicsBox;

        public MovingPlatformController(Engine engine)
        {
            this.engine = engine;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;

            var texture = engine.ContentManager.Load<ITexture>("Textures", "Dirt");
            var shader = engine.ContentManager.Load<Shader>("Shaders", "Voxel");
            material = new Material(engine, shader);
            material.SetTexture("Texture", texture);
            renderable = new BasicRenderable<VertexPositionNormalTexCoordMaterial>(engine, material);

            var vertices = ShapeBuilder.BuildCubeVertices().Select(v => new VertexPositionNormalTexCoordMaterial(v.Position, v.Normal, v.TexCoord, 1)).ToArray();
            var indices = ShapeBuilder.BuildCubeIndicies();
            var mesh = new Mesh<VertexPositionNormalTexCoordMaterial>(vertices, indices, PrimitiveType.TriangleList);
            renderable.SetMesh(VertexPositionNormalTexCoordMaterial.VertexLayoutDescription, mesh);

            entity.AddComponent(renderable);
            entity.Transform.Scale = new Vector3(PlatformSize.X, PlatformSize.Y, PlatformSize.Z);

            physicsBox = new PhysicsBoxComponent(new Vector3(PlatformSize.X, PlatformSize.Y, PlatformSize.Z), PhysicsInteractivity.Kinematic);
            entity.AddComponent(physicsBox);
        }

        public void DetachedFromEntity()
        {
            entity.RemoveComponent(renderable);
            renderable.Dispose();
            entity.RemoveComponent(physicsBox);

            entity = null;
        }

        public void Update()
        {
            physicsBox.LinearVelocity = MovementDirection * MovementSpeed;
        }
    }
}
