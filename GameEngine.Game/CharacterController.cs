using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameEngine.Core;
using GameEngine.Core.Camera;
using GameEngine.Core.Entities;
using GameEngine.Core.Input;
using GameEngine.Core.Physics;

namespace GameEngine.Game
{
    public class CharacterController : IComponent
    {
        public PhysicsCapsuleComponent PhysicsComponent { get; private set; }
        public float CameraHeightOffset => (height / 2f) + (radius / 2f);
        public float WalkSpeed { get; set; } = 5f;
        public float SprintSpeed { get; set; } = 10f;
        public float JumpForce { get; set; } = 5f;
        public float JumpMinTimeBetween => 0.75f;
        public float GroundedRayLength => 0.1f;
        public float DecelerationSpeed => 10f;

        private readonly Engine engine;
        private readonly Scene scene;
        private Vector3 initalPosition;
        private float height;
        private float radius;
        private float mass;
        private Entity entity;
        private ICamera camera;
        private bool isGrounded;
        private double lastJumpSeconds;
        private List<RayHit> supportHits;

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
            supportHits = new List<RayHit>();
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
            CheckCharacterIsGrounded();
            var hasMainSupport = GetMainSupport(out var mainSupportHit);
            var keyboard = engine.InputManager.Keyboard;

            // Calculate movement direction
            var headingDirection = Vector3.Normalize(new Vector3(camera.ViewDirection.X, 0, camera.ViewDirection.Z));
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

            // Calculate main support object velocity
            Vector3 supportVelocity;
            if (hasMainSupport)
            {
                supportVelocity = new Vector3(
                    mainSupportHit.PhysicsComponent.LinearVelocity.X,
                    0,
                    mainSupportHit.PhysicsComponent.LinearVelocity.Z);
            }
            else
            {
                supportVelocity = Vector3.Zero;
            }

            // Calculate velocities
            var targetVelocity = new Vector3(
                movementDirection.X * movementSpeed,
                0,
                movementDirection.Z * movementSpeed);

            var currentVelocity = PhysicsComponent.LinearVelocity;
            var currentHorizontalVelocity = new Vector3(
                    currentVelocity.X,
                    0,
                    currentVelocity.Z);
            if (targetVelocity.Length() > 0)
            {
                var targetVelocityLength = targetVelocity.Length();
                var currentVelocityLength = currentHorizontalVelocity.Length();
                var diffBetweenCurrentTarget = targetVelocityLength - currentVelocityLength;

                targetVelocity.X *= diffBetweenCurrentTarget;
                targetVelocity.Z *= diffBetweenCurrentTarget;
            }
            else if (isGrounded)
            {
                targetVelocity = (Vector3.Zero - currentHorizontalVelocity + supportVelocity) * DecelerationSpeed;
            }

            // Apply jump after
            if (keyboard.WasKeyPressed(Keys.Space) && isGrounded && engine.GameTimeTotal.TotalSeconds - lastJumpSeconds > JumpMinTimeBetween)
            {
                targetVelocity.Y += JumpForce * mass;
                lastJumpSeconds = engine.GameTimeTotal.TotalSeconds;
            }

            // Apply the impulse
            if (targetVelocity.Length() > 0)
            {
                PhysicsComponent.ApplyImpulse(targetVelocity);
            }

            camera.Position = entity.Transform.Position + (Vector3.UnitY * CameraHeightOffset);
        }

        private void CheckCharacterIsGrounded()
        {
            const int rayCount = 8;

            isGrounded = false;
            supportHits.Clear();

            for (int i = 0; i < rayCount; i++)
            {
                var delta = i / (float)rayCount * MathF.PI * 2f;
                var xPos = MathF.Sin(delta);
                var zPos = MathF.Cos(delta);
                var playerOffset = new Vector3(xPos * radius, 0, zPos * radius);
                var rayStart = entity.Transform.Position - (Vector3.UnitY * height) + playerOffset;

                var rayHit = scene.PhysicsSystem.Raycast(rayStart, -Vector3.UnitY, GroundedRayLength,
                PhysicsInteractivity.Dynamic | PhysicsInteractivity.Kinematic | PhysicsInteractivity.Static,
                new[] { PhysicsComponent });

                if (rayHit.DidHit)
                {
                    isGrounded = true;
                    supportHits.Add(rayHit);
                }
            }
        }

        private bool GetMainSupport(out RayHit hit)
        {
            hit = new RayHit();

            foreach (var rayHit in supportHits)
            {
                // TODO implement a more fancy heuristic here
                hit = rayHit;
                return true;
            }

            return false;
        }
    }
}
