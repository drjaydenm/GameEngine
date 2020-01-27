using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Physics
{
    public interface IPhysicsSystem
    {
        bool DebugEnabled { get; set; }

        void Update();
        void Draw();

        RayHit Raycast(Vector3 origin, Vector3 direction, float maxDistance, PhysicsInteractivity interactivity);
        RayHit Raycast(Vector3 origin, Vector3 direction, float maxDistance, PhysicsInteractivity interactivity, PhysicsComponent[] ignoreComponents);

        void RegisterComponent(PhysicsComponent component);
        void DeregisterComponent(PhysicsComponent component);
        void UpdateComponent(PhysicsComponent component);
    }
}
