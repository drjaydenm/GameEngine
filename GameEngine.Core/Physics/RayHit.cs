using System.Numerics;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Physics
{
    public struct RayHit
    {
        public Entity Entity;
        public PhysicsComponent PhysicsComponent;
        public float HitDistance;
        public Vector3 Position;
        public Vector3 Normal;
        public bool DidHit;

        public RayHit(Entity entity, PhysicsComponent physicsComponent, float hitDistance, Vector3 position, Vector3 normal, bool didHit)
        {
            Entity = entity;
            PhysicsComponent = physicsComponent;
            HitDistance = hitDistance;
            Position = position;
            Normal = normal;
            DidHit = didHit;
        }
    }
}
