using System.Numerics;

namespace GameEngine.Core.Entities
{
    public struct BoxCompoundShape
    {
        public Vector3 RelativeOffset { get; set; }
        public Vector3 Size { get; set; }

        public BoxCompoundShape(Vector3 relativeOffset, Vector3 size)
        {
            RelativeOffset = relativeOffset;
            Size = size;
        }
    }

    public struct SphereCompoundShape
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
        public IReadOnlyList<BoxCompoundShape> BoxCompoundShapes => boxCompoundShapes;
        public IReadOnlyList<SphereCompoundShape> SphereCompoundShapes => sphereCompoundShapes;

        private List<BoxCompoundShape> boxCompoundShapes;
        private List<SphereCompoundShape> sphereCompoundShapes;

        public PhysicsCompoundComponent(PhysicsInteractivity interactivity) : base(interactivity)
        {
            boxCompoundShapes = new List<BoxCompoundShape>();
            sphereCompoundShapes = new List<SphereCompoundShape>();
        }

        public void AddBox(Vector3 relativeOffset, Vector3 size)
        {
            boxCompoundShapes.Add(new BoxCompoundShape(relativeOffset, size));
        }

        public void AddSphere(Vector3 relativeOffset, float radius)
        {
            sphereCompoundShapes.Add(new SphereCompoundShape(relativeOffset, radius));
        }
    }
}
