using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using GameEngine.Core.Entities;

namespace GameEngine.Core.Physics.BepuPhysics
{
    public struct DefaultRayHitHandler : IRayHitHandler
    {
        public CollidableReference Collidable;
        public float T;
        public Vector3 Position;
        public Vector3 Normal;
        public bool Hit;

        public PhysicsInteractivity InteractivityFilter;
        public CollidableReference[] IgnoreCollidables;

        public DefaultRayHitHandler(PhysicsInteractivity interactivityFilter, CollidableReference[] ignoreCollidables)
        {
            Collidable = default;
            T = float.MaxValue;
            Position = default;
            Normal = default;
            Hit = false;

            InteractivityFilter = interactivityFilter;
            IgnoreCollidables = ignoreCollidables;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            return AllowTest(collidable, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return ((collidable.Mobility == CollidableMobility.Dynamic && (InteractivityFilter & PhysicsInteractivity.Dynamic) != 0)
                    || (collidable.Mobility == CollidableMobility.Kinematic && (InteractivityFilter & PhysicsInteractivity.Kinematic) != 0)
                    || (collidable.Mobility == CollidableMobility.Static && (InteractivityFilter & PhysicsInteractivity.Static) != 0))
                && (IgnoreCollidables == null ||
                    (IgnoreCollidables != null && !IgnoreCollidables.Contains(collidable)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
        {
            if (t < maximumT)
                maximumT = t;

            if (t < T)
            {
                Collidable = collidable;
                T = t;
                Position = ray.Origin + (ray.Direction * t);
                Normal = normal;
                Hit = true;
            }
        }
    }
}
