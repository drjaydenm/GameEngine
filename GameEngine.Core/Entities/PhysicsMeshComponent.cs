using System.Numerics;

namespace GameEngine.Core.Entities
{
    public class PhysicsMeshComponent : PhysicsComponent
    {
        public Mesh<Vector3> Mesh { get; }

        public PhysicsMeshComponent(Mesh<Vector3> mesh) : base (PhysicsInteractivity.Static)
        {
            Mesh = mesh;
        }
    }
}
