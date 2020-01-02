using System;
using System.Linq;
using System.Numerics;
using GameEngine.Core;
using GameEngine.Core.Camera;
using GameEngine.Core.Entities;
using GameEngine.Core.Input;

namespace GameEngine.Game
{
    public class CharacterController : IComponent
    {
        public PhysicsCapsuleComponent PhysicsComponent => capsuleComponent;
        public float CameraHeightOffset => height / 2f;

        private readonly Engine engine;
        private Vector3 initalPosition;
        private float height;
        private float radius;
        private float mass;
        private PhysicsCapsuleComponent capsuleComponent;
        private Entity entity;
        private ICamera camera;
        private float speed = 0.3f;
        private float jumpForce = 5;

        public CharacterController(Engine engine, Vector3 position, float height, float radius, float mass)
        {
            this.engine = engine;
            initalPosition = position;
            this.height = height;
            this.radius = radius;
            this.mass = mass;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;
            capsuleComponent = new PhysicsCapsuleComponent(radius, height, PhysicsInteractivity.Dynamic)
            {
                Mass = mass,
                FreezeRotation = true
            };

            entity.AddComponent(capsuleComponent);
            entity.Transform.Position = initalPosition;

            camera = entity.GetComponentsOfType<ICamera>()?.First();
        }

        public void DetachedFromEntity()
        {
            entity.RemoveComponent(capsuleComponent);
        }

        public void Update()
        {
            var keyboard = engine.InputManager.Keyboard;

            var headingDirection = new Vector3(camera.ViewDirection.X, 0, camera.ViewDirection.Z);
            var movementDirection = Vector3.Zero;
            if (keyboard.WasKeyDown(Keys.A))
            {
                movementDirection += Vector3.Cross(Vector3.UnitY, headingDirection);
            }
            if (keyboard.WasKeyDown(Keys.D))
            {
                movementDirection -= Vector3.Cross(Vector3.UnitY, headingDirection);
            }
            if (keyboard.WasKeyDown(Keys.W))
            {
                movementDirection += headingDirection;
            }
            if (keyboard.WasKeyDown(Keys.S))
            {
                movementDirection -= headingDirection;
            }
            var movementDirectionLengthSquared = movementDirection.LengthSquared();
            if (movementDirectionLengthSquared > 0)
            {
                movementDirection /= MathF.Sqrt(movementDirectionLengthSquared);
            }

            var targetVelocity = new Vector3(
                movementDirection.X * speed,
                0,
                movementDirection.Z * speed);

            if (keyboard.WasKeyPressed(Keys.Space))
            {
                targetVelocity.Y += jumpForce;
            }

            if (targetVelocity.LengthSquared() > 0)
            {
                capsuleComponent.ApplyImpulse(targetVelocity * mass);
            }

            camera.Position = entity.Transform.Position + (Vector3.UnitY * CameraHeightOffset);
        }
    }
}
