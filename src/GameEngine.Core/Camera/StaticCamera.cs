using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Camera
{
    public class StaticCamera : ICamera
    {
        public Matrix4x4 View { get; private set; }
        public Matrix4x4 Projection { get; private set; }
        public Quaternion Rotation { get; private set; }
        public float NearClip { get; private set; }
        public float FarClip { get; private set; }
        public float FieldOfView { get; private set; }
        public float AspectRatio { get; private set; }

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

        private Vector3 position;
        private Vector3 direction;
        private Vector3 lookAt;

        public StaticCamera(Vector3 position, Vector3 lookAt, float fieldOfView, float nearClip, float farClip, float aspectRatio)
        {
            this.position = position;
            this.lookAt = lookAt;
            FieldOfView = fieldOfView;
            NearClip = nearClip;
            FarClip = farClip;
            AspectRatio = aspectRatio;

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
            // Empty as camera doesn't move :)
        }

        private void Recalculate()
        {
            direction = Vector3.Normalize(lookAt - position);
            View = Matrix4x4.CreateLookAt(position, lookAt, Vector3.UnitY);
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearClip, FarClip);
            Rotation = Quaternion.CreateFromYawPitchRoll(direction.X, direction.Y, direction.Z);
        }
    }
}
