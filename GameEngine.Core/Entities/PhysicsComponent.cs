using System.Numerics;
using GameEngine.Core.Physics;

namespace GameEngine.Core.Entities
{
    public abstract class PhysicsComponent : IComponent
    {
        public PhysicsInteractivity Interactivity { get; }
        public Vector3 PositionOffset { get; set; }
        public float Mass { get; set; }

        public Vector3 LinearVelocity
        {
            get { return Body.LinearVelocity; }
            set { Body.LinearVelocity = value; }
        }

        public Vector3 AngularVelocity
        {
            get { return Body.AngularVelocity; }
            set { Body.AngularVelocity = value; }
        }

        public float Friction
        {
            get { return friction; }
            set
            {
                friction = value;
                if (Body != null)
                    Body.Friction = friction;
            }
        }

        public bool FreezeRotation { get; set; }

        internal IPhysicsBody Body
        {
            get { return body; }
            set
            {
                body = value;

                if (value != null)
                    OnPhysicsBodyCreated();
            }
        }

        internal Entity Entity { get; private set; }

        private float friction;
        private IPhysicsBody body;

        public PhysicsComponent(PhysicsInteractivity interactivity)
        {
            Interactivity = interactivity;
            PositionOffset = Vector3.Zero;
            Mass = 1;
            Friction = 1;
        }

        public void AttachedToEntity(Entity entity)
        {
            Entity = entity;
        }

        public void DetachedFromEntity()
        {
            Entity = null;
        }

        public void Update()
        {
            if (body == null)
                return;

            Entity.Transform.SetPositionDirect(Body.Position - PositionOffset);
            Entity.Transform.SetRotationDirect(Body.Rotation);
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            Body.ApplyImpulse(impulse);
        }

        private void OnPhysicsBodyCreated()
        {
            Body.Friction = Friction;
        }
    }
}
