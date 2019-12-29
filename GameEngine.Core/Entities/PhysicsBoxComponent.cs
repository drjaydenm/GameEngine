using System.Numerics;

namespace GameEngine.Core.Entities
{
    public class PhysicsBoxComponent : PhysicsComponent
    {
        public Vector3 Size { get; }

        public PhysicsBoxComponent(Vector3 size, PhysicsInteractivity interactivity) : base (interactivity)
        {
            Size = size;
        }
    }
}
