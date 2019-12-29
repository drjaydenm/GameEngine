using System.Linq;
using System.Numerics;

namespace GameEngine.Core.Entities
{
    public class Transform
    {
        public Vector3 Position
        {
            get { return InternalPosition; }
            set
            {
                InternalPosition = value;
                var physicsComponent = entity.GetComponentsOfType<PhysicsComponent>().FirstOrDefault();
                if (physicsComponent != null)
                {
                    physicsComponent.Body.Position = InternalPosition + physicsComponent.PositionOffset;
                }
            }
        }

        public Quaternion Rotation
        {
            get { return InternalRotation; }
            set
            {
                InternalRotation = value;
                var physicsComponent = entity.GetComponentsOfType<PhysicsComponent>().FirstOrDefault();
                if (physicsComponent != null)
                {
                    physicsComponent.Body.Rotation = InternalRotation;
                }
            }
        }

        public Vector3 Scale { get; set; }

        internal Vector3 InternalPosition { get; set; }
        internal Quaternion InternalRotation { get; set; }

        private readonly Entity entity;

        public Transform(Entity entity)
        {
            this.entity = entity;

            InternalPosition = Vector3.Zero;
            InternalRotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }
}
