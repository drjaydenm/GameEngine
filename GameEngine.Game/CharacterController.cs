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
        public float WalkSpeed { get; set; } = 4f;
        public float SprintSpeed { get; set; } = 5.5f;
        public float JumpForce { get; set; } = 5f;
        public float JumpMinTimeBetween => 0.6f;
        public float GroundedRayLength => 0.15f;
        public float DecelerationSpeed => 15f;
        public float DirectionChangeFactor => 3f;
        public int GroundedRayCount => 8;
        public float GroundedRayRadius => radius * 0.9f;

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
            // Check for supports and if the character is grounded
            CheckCharacterIsGrounded();
            var hasMainSupport = GetMainSupport(out var mainSupportHit);
            var keyboard = engine.InputManager.Keyboard;

            // Calculate movement direction
            var movementDirection = GetInputMovementDirection();

            // Calculate movement speed
            var movementSpeed = WalkSpeed;
            if (keyboard.WasKeyDown(Keys.LeftShift))
            {
                movementSpeed = SprintSpeed;
            }

            // Calculate main support object velocity
            var supportVelocity = Vector3.Zero;
            if (hasMainSupport)
            {
                supportVelocity = new Vector3(
                    mainSupportHit.PhysicsComponent.LinearVelocity.X,
                    0,
                    mainSupportHit.PhysicsComponent.LinearVelocity.Z);
            }

            // Calculate velocities
            var targetMovementVelocity = movementDirection * movementSpeed;

            var currentVelocity = PhysicsComponent.LinearVelocity;
            var currentHorizontalVelocity = new Vector3(
                    currentVelocity.X,
                    0,
                    currentVelocity.Z);

            // If the player wants to try and move the character
            if (targetMovementVelocity.Length() > 0)
            {
                var targetVelocityLength = targetMovementVelocity.Length();
                var currentVelocityLength = (currentHorizontalVelocity - supportVelocity).Length();
                var diffBetweenCurrentTarget = Math.Max(targetVelocityLength - currentVelocityLength, 0);

                targetMovementVelocity.X *= diffBetweenCurrentTarget;
                targetMovementVelocity.Z *= diffBetweenCurrentTarget;

                // Subtract off a portion of the current direction to help change target directions
                targetMovementVelocity.X -= currentVelocity.X * DirectionChangeFactor;
                targetMovementVelocity.Z -= currentVelocity.Z * DirectionChangeFactor;

                // Add on the support velocity
                targetMovementVelocity.X += supportVelocity.X;
                targetMovementVelocity.Z += supportVelocity.Z;
            }
            else if (isGrounded)
            {
                // No desired player movement, so approach the support velocity
                targetMovementVelocity = (Vector3.Zero - currentHorizontalVelocity + supportVelocity) * DecelerationSpeed;
            }

            // Apply jump after
            if (keyboard.WasKeyPressed(Keys.Space) && isGrounded && engine.GameTimeTotal.TotalSeconds - lastJumpSeconds > JumpMinTimeBetween)
            {
                targetMovementVelocity.Y += JumpForce * mass;
                lastJumpSeconds = engine.GameTimeTotal.TotalSeconds;
            }

            // Apply the impulse
            if (targetMovementVelocity.Length() > 0)
            {
                PhysicsComponent.ApplyImpulse(targetMovementVelocity);
            }

            camera.Position = entity.Transform.Position + (Vector3.UnitY * CameraHeightOffset);
        }

        private void CheckCharacterIsGrounded()
        {
            isGrounded = false;
            supportHits.Clear();

            for (int i = 0; i < GroundedRayCount; i++)
            {
                var delta = i / (float)GroundedRayCount * MathF.PI * 2f;
                var xPos = MathF.Sin(delta);
                var zPos = MathF.Cos(delta);
                var playerOffset = new Vector3(xPos * GroundedRayRadius, 0, zPos * GroundedRayRadius);
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

        private Vector3 GetInputMovementDirection()
        {
            var keyboard = engine.InputManager.Keyboard;

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

            // Make sure there is no vertical contribution
            movementDirection.Y = 0;

            // Normalize
            var movementDirectionLengthSquared = movementDirection.LengthSquared();
            if (movementDirectionLengthSquared > 0)
            {
                movementDirection /= MathF.Sqrt(movementDirectionLengthSquared);
            }

            return movementDirection;
        }
    }
}
