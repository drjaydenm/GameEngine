using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace GameEngine.Core.Physics.BepuPhysics
{
    public class BepuPhysicsBody : IPhysicsBody
    {
        public Vector3 Position
        {
            get
            {
                if (IsBody)
                    return BodyReference.Pose.Position;

                return StaticReference.Pose.Position;
            }
            set
            {
                if (IsBody)
                    BodyReference.Pose.Position = value;
                else
                    StaticReference.Pose.Position = value;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                if (IsBody)
                    return BodyReference.Pose.Orientation;
                else
                    return StaticReference.Pose.Orientation;
            }
            set
            {
                var o = new Quaternion(value.X, value.Y, value.Z, value.W);

                if (IsBody)
                    BodyReference.Pose.Orientation = o;
                else
                    StaticReference.Pose.Orientation = o;
            }
        }

        public Vector3 LinearVelocity
        {
            get
            {
                if (IsBody)
                    return BodyReference.Velocity.Linear;

                return Vector3.Zero;
            }
            set
            {
                if (IsBody)
                {
                    BodyReference.Velocity.Linear = value;
                    AwakenIfNeeded();
                }
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                if (IsBody)
                    return BodyReference.Velocity.Angular;

                return Vector3.Zero;
            }
            set
            {
                if (IsBody)
                {
                    BodyReference.Velocity.Angular = value;
                    AwakenIfNeeded();
                }
            }
        }

        public float Mass
        {
            get
            {
                if (IsBody)
                    return BodyReference.LocalInertia.InverseMass;

                return 0;
            }
            set
            {
                if (IsBody)
                    BodyReference.LocalInertia.InverseMass = value;
            }
        }

        public float Friction { get; set; }

        internal BodyReference BodyReference { get; set; }
        internal StaticReference StaticReference { get; set; }
        internal TypedIndex ShapeIndex { get; set; }
        internal bool IsBody { get; private set; }

        private readonly Simulation simulation;

        public BepuPhysicsBody(Simulation simulation, bool isBody)
        {
            this.simulation = simulation;
            IsBody = isBody;
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            AwakenIfNeeded();
            BodyReference.ApplyImpulse(impulse, Vector3.Zero);
        }

        private void AwakenIfNeeded()
        {
            if (!BodyReference.Awake)
            {
                simulation.Awakener.AwakenBody(BodyReference.Handle);
            }
        }
    }
}
