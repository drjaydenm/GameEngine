using System.Collections.Generic;
using System.Numerics;

namespace GameEngine.Core.Entities
{
    public interface ICompoundShape
    {
        public Vector3 RelativeOffset { get; set; }
    }

    public struct BoxCompoundShape : ICompoundShape
    {
        public Vector3 RelativeOffset { get; set; }
        public Vector3 Size { get; set; }

        public BoxCompoundShape(Vector3 relativeOffset, Vector3 size)
        {
            RelativeOffset = relativeOffset;
            Size = size;
        }
    }

    public struct SphereCompoundShape : ICompoundShape
    {
        public Vector3 RelativeOffset { get; set; }
        public float Radius { get; set; }

        public SphereCompoundShape(Vector3 relativeOffset, float radius)
        {
            RelativeOffset = relativeOffset;
            Radius = radius;
        }
    }

    public class PhysicsCompoundComponent : PhysicsComponent
    {
        public IReadOnlyList<ICompoundShape> CompoundShapes => compoundShapes;

        private List<ICompoundShape> compoundShapes;

        public PhysicsCompoundComponent(PhysicsInteractivity interactivity) : base(interactivity)
        {
            compoundShapes = new List<ICompoundShape>();
        }

        public void AddBox(Vector3 relativeOffset, Vector3 size)
        {
            compoundShapes.Add(new BoxCompoundShape(relativeOffset, size));
        }

        public void AddSphere(Vector3 relativeOffset, float radius)
        {
            compoundShapes.Add(new SphereCompoundShape(relativeOffset, radius));
        }
    }
}
