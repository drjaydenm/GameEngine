using System;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace GameEngine.Core.Physics.BepuPhysics
{
    public unsafe struct DefaultNarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public SpringSettings ContactSpringiness;
        public BepuPhysicsSystem PhysicsSystem;

        public DefaultNarrowPhaseCallbacks(BepuPhysicsSystem system)
        {
            ContactSpringiness = new SpringSettings();
            PhysicsSystem = system;
        }

        public void Initialize(Simulation simulation)
        {
            //Use a default if the springiness value wasn't initialized.
            if (ContactSpringiness.AngularFrequency == 0 && ContactSpringiness.TwiceDampingRatio == 0)
                ContactSpringiness = new SpringSettings(30, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            ConfigureMaterial(pair, out pairMaterial);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConfigureMaterial(CollidablePair pair, out PairMaterialProperties pairMaterial)
        {
            if (pair.A.Mobility != CollidableMobility.Dynamic && pair.B.Mobility != CollidableMobility.Dynamic)
            {
                pairMaterial = new PairMaterialProperties();
                return;
            }

            var bodyA = PhysicsSystem.CollidableHandleToBody[new Tuple<CollidableMobility, int>(pair.A.Mobility, pair.A.Handle)];
            var bodyB = PhysicsSystem.CollidableHandleToBody[new Tuple<CollidableMobility, int>(pair.B.Mobility, pair.B.Handle)];

            pairMaterial.FrictionCoefficient = bodyA.Friction * bodyB.Friction;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = ContactSpringiness;
        }
    }
}
