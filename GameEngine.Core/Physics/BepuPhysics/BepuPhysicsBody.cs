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
                if (isBody)
                    return BodyReference.Pose.Position;

                return StaticReference.Pose.Position;
            }
            set
            {
                if (isBody)
                    BodyReference.Pose.Position = value;
                else
                    StaticReference.Pose.Position = value;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                BepuUtilities.Quaternion o;

                if (isBody)
                    o = BodyReference.Pose.Orientation;
                else
                    o = StaticReference.Pose.Orientation;

                return new Quaternion(o.X, o.Y, o.Z, o.W);
            }
            set
            {
                var o = new BepuUtilities.Quaternion(value.X, value.Y, value.Z, value.W);

                if (isBody)
                    BodyReference.Pose.Orientation = o;
                else
                    StaticReference.Pose.Orientation = o;
            }
        }

        public Vector3 LinearVelocity
        {
            get
            {
                if (isBody)
                    return BodyReference.Velocity.Linear;

                return Vector3.Zero;
            }
            set
            {
                if (isBody)
                    BodyReference.Velocity.Linear = value;
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                if (isBody)
                    return BodyReference.Velocity.Angular;

                return Vector3.Zero;
            }
            set
            {
                if (isBody)
                    BodyReference.Velocity.Angular = value;
            }
        }

        public float Mass
        {
            get
            {
                if (isBody)
                    return BodyReference.LocalInertia.InverseMass;

                return 0;
            }
            set
            {
                if (isBody)
                    BodyReference.LocalInertia.InverseMass = value;
            }
        }

        internal BodyReference BodyReference { get; set; }
        internal StaticReference StaticReference { get; set; }
        internal TypedIndex ShapeIndex { get; set; }

        private readonly Simulation simulation;
        private readonly bool isBody;

        public BepuPhysicsBody(Simulation simulation, bool isBody)
        {
            this.simulation = simulation;
            this.isBody = isBody;
        }
    }
}
