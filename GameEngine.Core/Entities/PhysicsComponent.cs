﻿using System.Numerics;
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

        internal IPhysicsBody Body { get; set; }
        internal Entity Entity { get; set; }

        public PhysicsComponent(PhysicsInteractivity interactivity)
        {
            Interactivity = interactivity;
        }

        public void Update()
        {
            Entity.Transform.InternalPosition = Body.Position - PositionOffset;
            Entity.Transform.InternalRotation = Body.Rotation;
        }
    }
}
