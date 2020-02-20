using System.Numerics;

namespace GameEngine.Core.Entities
{
    public class PhysicsMeshComponent : PhysicsComponent
    {
        public Mesh<Vector3> Mesh { get; }
        public Vector3 MeshScale { get; }

        public PhysicsMeshComponent(Mesh<Vector3> mesh, Vector3 meshScale, PhysicsInteractivity interactivity) : base (interactivity)
        {
            Mesh = mesh;
            MeshScale = meshScale;
        }
    }
}
