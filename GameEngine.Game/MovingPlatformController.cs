﻿using System.Linq;
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
        private BasicRenderable<VertexPositionNormalMaterial> renderable;
        private Material material;
        private PhysicsBoxComponent physicsBox;

        public MovingPlatformController(Engine engine)
        {
            this.engine = engine;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;

            material = new Material(engine, ShaderCode.VertexCode, ShaderCode.FragmentCode);
            renderable = new BasicRenderable<VertexPositionNormalMaterial>(engine, material);

            var vertices = ShapeBuilder.BuildCubeVertices().Select(v => new VertexPositionNormalMaterial(v, Vector3.UnitY, 1)).ToArray();
            var indices = ShapeBuilder.BuildCubeIndicies();
            var mesh = new Mesh<VertexPositionNormalMaterial>(ref vertices, ref indices);
            renderable.SetMesh(VertexPositionNormalMaterial.VertexLayoutDescription, mesh);

            entity.AddComponent(renderable);

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

            renderable.SetWorldTransform(Matrix4x4.CreateScale(PlatformSize.X, PlatformSize.Y, PlatformSize.Z) * Matrix4x4.CreateTranslation(entity.Transform.Position));
        }
    }
}