using System.Numerics;
using GameEngine.Core.Entities;
using GameEngine.Core.Input;

namespace GameEngine.Core.Camera
{
    public class DebugCamera : ICamera
    {
        private const float DEG_2_RAD = (float)Math.PI / 180f;

        public Matrix4x4 View { get; private set; }
        public Matrix4x4 Projection { get; private set; }
        public Quaternion Rotation { get; private set; }
        public float NearClip { get; private set; }
        public float FarClip { get; private set; }
        public float FieldOfView { get; private set; }
        public float AspectRatio => engine.Window.AspectRatio;
        public bool DisableRotation { get; set; }

        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                Recalculate();
            }
        }
        public Vector3 ViewDirection
        {
            get { return direction; }
            set
            {
                direction = value;
                Recalculate();
            }
        }

        private float mouseSensitivity = 0.1f;
        private float movementSpeed = 15f;
        private float slowMovementSpeed = 2f;
        private float fastMovementSpeed = 50f;
        private float accumPitch { get; set; }
        private float maxPitch = (float)Math.PI / 2 - .05f;
        private Vector3 position;
        private Vector3 direction;
        private Engine engine;

        public DebugCamera(Vector3 position, Vector3 direction, float fieldOfView, float nearClip, float farClip, Engine engine)
        {
            this.engine = engine;
            this.position = position;
            this.direction = direction;
            FieldOfView = fieldOfView;
            NearClip = nearClip;
            FarClip = farClip;
            Rotation = Quaternion.Identity;
            Recalculate();
        }

        public void AttachedToEntity(Entity entity)
        {
        }

        public void DetachedFromEntity()
        {
        }

        public void Update()
        {
            var keyboard = engine.InputManager.Keyboard;
            var mouse = engine.InputManager.Mouse;
            var mouseDelta = new Vector2(-mouse.PositionDelta.X, mouse.PositionDelta.Y) * mouseSensitivity * (float)engine.GameTimeElapsed.TotalSeconds;

            if (DisableRotation)
                mouseDelta = Vector2.Zero;

            // Now add up the pitch
            accumPitch += mouseDelta.Y;

            if (accumPitch > maxPitch)
            {
                mouseDelta.Y = maxPitch - (accumPitch - mouseDelta.Y);
                accumPitch = maxPitch;
            }

            if (accumPitch < -maxPitch)
            {
                mouseDelta.Y = -maxPitch - (accumPitch - mouseDelta.Y);
                accumPitch = -maxPitch;
            }

            float heading = mouseDelta.X;
            float pitch = mouseDelta.Y;

            if (heading != 0.0f)
                Rotation = Quaternion.Concatenate(Rotation, Quaternion.CreateFromAxisAngle(Vector3.UnitY, heading));

            if (pitch != 0.0f)
                Rotation = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch), Rotation);

            var view = Matrix4x4.CreateFromQuaternion(Rotation);
            direction = Vector3.Transform(Vector3.UnitZ, view);

            var cameraMovement = Vector3.Zero;
            if (keyboard.WasKeyDown(Keys.A))
            {
                cameraMovement += Vector3.Cross(Vector3.UnitY, direction);
            }
            if (keyboard.WasKeyDown(Keys.D))
            {
                cameraMovement -= Vector3.Cross(Vector3.UnitY, direction);
            }
            if (keyboard.WasKeyDown(Keys.W))
            {
                cameraMovement += direction;
            }
            if (keyboard.WasKeyDown(Keys.S))
            {
                cameraMovement -= direction;
            }

            if (cameraMovement != Vector3.Zero)
            {
                cameraMovement = Vector3.Normalize(cameraMovement);
            }

            var actualMovementSpeed = movementSpeed;
            if (keyboard.IsKeyDown(Keys.LeftControl))
            {
                actualMovementSpeed = slowMovementSpeed;
            }
            if (keyboard.IsKeyDown(Keys.LeftShift))
            {
                actualMovementSpeed = fastMovementSpeed;
            }
            Position += cameraMovement * actualMovementSpeed * (float)engine.GameTimeElapsed.TotalSeconds;
        }

        private void Recalculate()
        {
            View = Matrix4x4.CreateLookAt(position, position + direction, Vector3.UnitY);
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView * DEG_2_RAD, AspectRatio, NearClip, FarClip);
           
        }
    }
}
