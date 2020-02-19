using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameEngine.Core.Entities
{
    public class Transform
    {
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                var physicsComponent = entity.GetComponentsOfType<PhysicsComponent>().FirstOrDefault();
                if (physicsComponent != null)
                {
                    physicsComponent.Body.Position = position + physicsComponent.PositionOffset;
                }
                UpdateMatrix();
            }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                var physicsComponent = entity.GetComponentsOfType<PhysicsComponent>().FirstOrDefault();
                if (physicsComponent != null)
                {
                    physicsComponent.Body.Rotation = rotation;
                }
                UpdateMatrix();
            }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                UpdateMatrix();
            }
        }

        public Matrix4x4 TransformMatrix { get; private set; }

        private readonly Entity entity;
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        public Transform(Entity entity)
        {
            this.entity = entity;

            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;

            UpdateMatrix();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetPositionDirect(Vector3 position)
        {
            this.position = position;
            UpdateMatrix();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetRotationDirect(Quaternion rotation)
        {
            this.rotation = rotation;
            UpdateMatrix();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetScaleDirect(Vector3 scale)
        {
            this.scale = scale;
            UpdateMatrix();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMatrix()
        {
            TransformMatrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }
    }
}
