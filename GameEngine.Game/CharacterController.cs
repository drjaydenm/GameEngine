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
        public PhysicsCapsuleComponent PhysicsComponent { get; private set; }
        public float CameraHeightOffset => (height / 2f) + (radius / 2f);
        public float WalkSpeed { get; set; } = 7f;
        public float SprintSpeed { get; set; } = 10f;
        public float JumpForce { get; set; } = 5f;

        private readonly Engine engine;
        private readonly Scene scene;
        private Vector3 initalPosition;
        private float height;
        private float radius;
        private float mass;
        private Entity entity;
        private ICamera camera;
        private bool isGrounded;

        /// <summary>
        /// Create a new instance of the <c>CharacterController</c>
        /// </summary>
        /// <param name="engine">The engine instance</param>
        /// <param name="position">The initial starting position for the character</param>
        /// <param name="height">The height of the character capsule, excluding the radius on top and bottom - e.g. height of 1 and radius 0.5 actually gives a character of height 2</param>
        /// <param name="radius">The radius for the character capsule, also gets added to the height on the bottom and top semi-spheres</param>
        /// <param name="mass">The mass of the character</param>
        public CharacterController(Engine engine, Scene scene, Vector3 position, float height, float radius, float mass)
        {
            this.engine = engine;
            this.scene = scene;
            initalPosition = position;
            this.height = height;
            this.radius = radius;
            this.mass = mass;
        }

        public void AttachedToEntity(Entity entity)
        {
            this.entity = entity;
            PhysicsComponent = new PhysicsCapsuleComponent(radius, height, PhysicsInteractivity.Dynamic)
            {
                Mass = mass,
                FreezeRotation = true,
                Friction = 0
            };

            entity.AddComponent(PhysicsComponent);
            entity.Transform.Position = initalPosition;

            camera = entity.GetComponentsOfType<ICamera>()?.First();
        }

        public void DetachedFromEntity()
        {
            entity.RemoveComponent(PhysicsComponent);
        }

        public void Update()
        {
            CheckCharacterGrounded();
            var keyboard = engine.InputManager.Keyboard;

            // Calculate movement direction
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

            // Calculate movement speed
            var movementSpeed = WalkSpeed;
            if (keyboard.WasKeyDown(Keys.LeftShift))
            {
                movementSpeed = SprintSpeed;
            }

            // Calculate velocities
            var targetVelocity = new Vector3(
                movementDirection.X * movementSpeed,
                0,
                movementDirection.Z * movementSpeed);
            
            if (targetVelocity.Length() > 0)
            {
                var currentVelocity = new Vector3(
                    PhysicsComponent.LinearVelocity.X,
                    0,
                    PhysicsComponent.LinearVelocity.Z);

                var targetVelocityLength = targetVelocity.Length();
                var currentVelocityLength = currentVelocity.Length();
                var diffBetweenCurrentTarget = targetVelocityLength - currentVelocityLength;

                targetVelocity.X *= diffBetweenCurrentTarget;
                targetVelocity.Z *= diffBetweenCurrentTarget;
            }

            // Apply jump after
            if (keyboard.WasKeyPressed(Keys.Space) && isGrounded)
            {
                targetVelocity.Y += JumpForce * mass;
            }

            // Apply the impulse
            if (targetVelocity.Length() > 0)
            {
                PhysicsComponent.ApplyImpulse(targetVelocity);
            }

            camera.Position = entity.Transform.Position + (Vector3.UnitY * CameraHeightOffset);
        }

        private void CheckCharacterGrounded()
        {
            const int rayCount = 8;

            isGrounded = false;
            for (int i = 0; i < rayCount; i++)
            {
                var delta = i / (float)rayCount * MathF.PI * 2;
                var xPos = MathF.Sin(delta);
                var zPos = MathF.Cos(delta);
                var playerOffset = new Vector3(xPos * 0.5f, 0, zPos * 0.5f);
                var rayStart = entity.Transform.Position - (Vector3.UnitY * 1f) + playerOffset;

                var rayHit = scene.PhysicsSystem.Raycast(rayStart, -Vector3.UnitY, 0.1f,
                PhysicsInteractivity.Dynamic | PhysicsInteractivity.Kinematic | PhysicsInteractivity.Static,
                new[] { PhysicsComponent });

                if (rayHit.DidHit)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }
}
