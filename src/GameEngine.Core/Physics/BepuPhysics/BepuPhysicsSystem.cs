using System.Collections.Concurrent;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using GameEngine.Core.Entities;
using GameEngine.Core.World;
using BepuMesh = BepuPhysics.Collidables.Mesh;

namespace GameEngine.Core.Physics.BepuPhysics
{
    public class BepuPhysicsSystem : IPhysicsSystem, IDisposable
    {
        private struct AsyncShapeCreationResult
        {
            public PhysicsComponent Component;
            public BodyInertia Inertia;
            public TypedIndex ShapeIndex;
        }

        public bool DebugEnabled { get; set; }

        internal Dictionary<int, BepuPhysicsBody> BodyHandleToBody;
        internal Dictionary<int, BepuPhysicsBody> StaticHandleToBody;

        private readonly Engine engine;
        private readonly Scene scene;
        private BufferPool bufferPool;
        private Simulation simulation;
        private DefaultThreadDispatcher threadDispatcher;
        private Dictionary<PhysicsComponent, BepuPhysicsBody> registeredComponents;
        private ConcurrentQueue<AsyncShapeCreationResult> asyncShapeQueue;
        private HashSet<PhysicsComponent> pendingAsyncShapes;

        public BepuPhysicsSystem(Engine engine, Scene scene, Vector3 gravity)
        {
            this.engine = engine;
            this.scene = scene;

            bufferPool = new BufferPool();
            threadDispatcher = new DefaultThreadDispatcher(Environment.ProcessorCount);
            simulation = Simulation.Create(bufferPool,
                new DefaultNarrowPhaseCallbacks(this),
                new DefaultPoseIntegratorCallbacks(gravity),
                new PositionLastTimestepper());
            registeredComponents = new Dictionary<PhysicsComponent, BepuPhysicsBody>();
            asyncShapeQueue = new ConcurrentQueue<AsyncShapeCreationResult>();
            pendingAsyncShapes = new HashSet<PhysicsComponent>();

            BodyHandleToBody = new Dictionary<int, BepuPhysicsBody>();
            StaticHandleToBody = new Dictionary<int, BepuPhysicsBody>();
        }

        public void DeregisterComponent(PhysicsComponent component)
        {
            if (registeredComponents.ContainsKey(component))
            {
                var body = registeredComponents[component];
                registeredComponents.Remove(component);

                if (component.Interactivity == PhysicsInteractivity.Static)
                {
                    StaticHandleToBody.Remove(body.StaticReference.Handle.Value);
                }
                else
                {
                    BodyHandleToBody.Remove(body.BodyReference.Handle.Value);
                }

                simulation.Shapes.RecursivelyRemoveAndDispose(body.ShapeIndex, bufferPool);
                if (component.Interactivity == PhysicsInteractivity.Static)
                {
                    simulation.Statics.Remove(body.StaticReference.Handle);
                }
                else
                {
                    simulation.Bodies.Remove(body.BodyReference.Handle);
                }
            }

            if (pendingAsyncShapes.Contains(component))
            {
                pendingAsyncShapes.Remove(component);
            }
        }

        public void RegisterComponent(PhysicsComponent component)
        {
            TypedIndex shapeIndex = default;
            BodyInertia inertia = default;
            var isAsyncShape = false;

            if (component is PhysicsBoxComponent boxComponent)
            {
                var boxShape = new Box(boxComponent.Size.X, boxComponent.Size.Y, boxComponent.Size.Z);
                shapeIndex = simulation.Shapes.Add(boxShape);

                if (component.Interactivity == PhysicsInteractivity.Dynamic)
                    boxShape.ComputeInertia(component.Mass, out inertia);
            }
            else if (component is PhysicsCapsuleComponent capsuleComponent)
            {
                var capsuleShape = new Capsule(capsuleComponent.Radius, capsuleComponent.Length);
                shapeIndex = simulation.Shapes.Add(capsuleShape);

                if (component.Interactivity == PhysicsInteractivity.Dynamic)
                    capsuleShape.ComputeInertia(component.Mass, out inertia);
            }
            else if (component is PhysicsCompoundComponent compoundComponent)
            {
                isAsyncShape = true;
                pendingAsyncShapes.Add(component);
                engine.Jobs.WhenIdle.EnqueueJob(() =>
                {
                    if (!pendingAsyncShapes.Contains(component))
                        return;

                    BuildCompoundShape(compoundComponent, out inertia, out shapeIndex);
                    asyncShapeQueue.Enqueue(new AsyncShapeCreationResult
                    {
                        Component = component,
                        Inertia = inertia,
                        ShapeIndex = shapeIndex
                    });
                });
            }
            else if (component is PhysicsChunkComponent chunkComponent)
            {
                isAsyncShape = true;
                pendingAsyncShapes.Add(component);
                engine.Jobs.WhenIdle.EnqueueJob(() =>
                {
                    if (!pendingAsyncShapes.Contains(component))
                        return;

                    BuildChunkShape(chunkComponent, out shapeIndex);
                    asyncShapeQueue.Enqueue(new AsyncShapeCreationResult
                    {
                        Component = component,
                        Inertia = inertia,
                        ShapeIndex = shapeIndex
                    });
                });
            }
            else if (component is PhysicsMeshComponent meshComponent)
            {
                var meshTriangles = meshComponent.Mesh.TransformAsTriangles();
                bufferPool.Take<Triangle>(meshTriangles.Length, out var triangles);

                for (int i = 0; i < meshTriangles.Length; ++i)
                {
                    triangles[i] = new Triangle(meshTriangles[i].Vertex1, meshTriangles[i].Vertex2, meshTriangles[i].Vertex3);
                }

                var mesh = new BepuMesh(triangles, meshComponent.MeshScale, bufferPool);
                shapeIndex = simulation.Shapes.Add(mesh);

                if (component.Interactivity == PhysicsInteractivity.Dynamic)
                    mesh.ComputeClosedInertia(component.Mass, out inertia);
            }

            if (!isAsyncShape)
                AddPhysicsBody(component, ref inertia, ref shapeIndex);
        }

        public void UpdateComponent(PhysicsComponent component)
        {
            if (component is PhysicsChunkComponent chunkComponent)
            {
                pendingAsyncShapes.Add(component);
                engine.Jobs.WhenIdle.EnqueueJob(() =>
                {
                    if (!pendingAsyncShapes.Contains(component))
                        return;

                    BuildChunkShape(chunkComponent, out var shapeIndex);
                    asyncShapeQueue.Enqueue(new AsyncShapeCreationResult
                    {
                        Component = component,
                        Inertia = default,
                        ShapeIndex = shapeIndex
                    });
                });
            }
        }

        public void Update()
        {
            while (asyncShapeQueue.TryDequeue(out var shape))
            {
                // The component could have been deregistered in the meantime
                if (!pendingAsyncShapes.Contains(shape.Component))
                    continue;

                if (registeredComponents.ContainsKey(shape.Component))
                    DeregisterComponent(shape.Component);

                AddPhysicsBody(shape.Component, ref shape.Inertia, ref shape.ShapeIndex);
            }

            simulation.Timestep((float)engine.GameTimeElapsed.TotalSeconds / 2f, threadDispatcher);
            simulation.Timestep((float)engine.GameTimeElapsed.TotalSeconds / 2f, threadDispatcher);
        }

        public void Draw()
        {
            if (!DebugEnabled)
                return;

            foreach (var kvp in registeredComponents)
            {
                var position = kvp.Key.Body.Position;
                var orientation = kvp.Key.Body.Rotation;

                if (kvp.Key is PhysicsBoxComponent boxComponent)
                {
                    var transform = Matrix4x4.CreateScale(boxComponent.Size) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(Color.Red, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
                else if (kvp.Key is PhysicsCapsuleComponent capsuleComponent)
                {
                    var transform = Matrix4x4.CreateScale(capsuleComponent.Radius, capsuleComponent.Length, capsuleComponent.Radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(Color.Orange, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
                else if (kvp.Key is PhysicsMeshComponent meshComponent)
                {
                    // TODO debug draw the actual mesh, for now just shows a 1x1x1 cube
                    var transform = Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(Color.Blue, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
                else if (kvp.Key is PhysicsCompoundComponent compoundComponent)
                {
                    // TODO debug draw the actual compound shape, for now just shows a 1x1x1 cube
                    /*foreach (var shape in compoundComponent.CompoundShapes)
                    {
                        var transform = Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position + shape.RelativeOffset);
                        engine.DebugGraphics.DrawCube(Color.Cyan, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                    }*/

                    var transform = Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(Color.Cyan, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
            }
        }

        public RayHit Raycast(Vector3 origin, Vector3 direction, float maxDistance, PhysicsInteractivity interactivity)
        {
            return Raycast(origin, direction, maxDistance, interactivity, null);
        }

        public RayHit Raycast(Vector3 origin, Vector3 direction, float maxDistance, PhysicsInteractivity interactivity, PhysicsComponent[] ignoreComponents)
        {
            var actualIgnoreComponents = ignoreComponents
                ?.Where(c => registeredComponents.ContainsKey(c))
                .Select(c => BodyToCollidableReference((BepuPhysicsBody)c.Body)).ToArray();

            var hitHandler = new DefaultRayHitHandler(interactivity, actualIgnoreComponents);
            simulation.RayCast(origin, direction, maxDistance, ref hitHandler);

            PhysicsComponent component = null;
            if (hitHandler.Hit)
            {
                BepuPhysicsBody body;
                if (hitHandler.Collidable.Mobility == CollidableMobility.Static)
                    body = StaticHandleToBody[hitHandler.Collidable.RawHandleValue];
                else
                    body = BodyHandleToBody[hitHandler.Collidable.RawHandleValue];

                component = registeredComponents.Where(c => c.Value == body).FirstOrDefault().Key;
            }

            return new RayHit(component?.Entity, component, hitHandler.T, hitHandler.Position, hitHandler.Normal, hitHandler.Hit);
        }

        public void Dispose()
        {
            simulation.Dispose();
            bufferPool.Clear();
            threadDispatcher.Dispose();
        }

        private void AddPhysicsBody(PhysicsComponent component, ref BodyInertia inertia, ref TypedIndex shapeIndex)
        {
            BepuPhysicsBody body;
            BodyHandle? bodyHandle = null;
            StaticHandle? staticHandle = null;

            if (component.FreezeRotation)
            {
                inertia.InverseInertiaTensor = new Symmetric3x3();
            }

            var collidable = new CollidableDescription(shapeIndex, 0.1f);
            var activity = new BodyActivityDescription(0.01f);

            if (component.Interactivity == PhysicsInteractivity.Dynamic)
            {
                var bodyDescription = BodyDescription.CreateDynamic(
                    component.Entity.Transform.Position + component.PositionOffset, inertia, collidable, activity);
                bodyHandle = simulation.Bodies.Add(bodyDescription);
            }
            else if (component.Interactivity == PhysicsInteractivity.Kinematic)
            {
                var bodyDescription = BodyDescription.CreateKinematic(
                    component.Entity.Transform.Position + component.PositionOffset, collidable, activity);
                bodyHandle = simulation.Bodies.Add(bodyDescription);
            }
            else if (component.Interactivity == PhysicsInteractivity.Static)
            {
                var staticDescription = new StaticDescription(component.Entity.Transform.Position + component.PositionOffset, collidable);
                staticHandle = simulation.Statics.Add(staticDescription);
            }

            body = new BepuPhysicsBody(simulation, bodyHandle.HasValue);
            body.ShapeIndex = shapeIndex;

            if (bodyHandle.HasValue)
                body.BodyReference = new BodyReference(bodyHandle.Value, simulation.Bodies);
            else if (staticHandle.HasValue)
                body.StaticReference = new StaticReference(staticHandle.Value, simulation.Statics);

            component.Body = body;
            registeredComponents.Add(component, body);

            if (component.Interactivity == PhysicsInteractivity.Static)
                StaticHandleToBody.Add(staticHandle.Value.Value, body);
            else
                BodyHandleToBody.Add(bodyHandle.Value.Value, body);
        }

        private CollidableReference BodyToCollidableReference(BepuPhysicsBody body)
        {
            if (body.IsBody)
                return new CollidableReference(body.BodyReference.Kinematic ? CollidableMobility.Kinematic : CollidableMobility.Dynamic, body.BodyReference.Handle);
            else
                return new CollidableReference(body.StaticReference.Handle);
        }

        internal Tuple<CollidableMobility, int> BodyToCollidableKey(BepuPhysicsBody body)
        {
            return new Tuple<CollidableMobility, int>(
                body.IsBody ? body.BodyReference.Kinematic ? CollidableMobility.Kinematic : CollidableMobility.Dynamic : CollidableMobility.Static,
                body.IsBody ? body.BodyReference.Handle.Value : body.StaticReference.Handle.Value);
        }

        private void BuildCompoundShape(PhysicsCompoundComponent compoundComponent, out BodyInertia inertia, out TypedIndex shapeIndex)
        {
            inertia = default;
            var compoundCount = compoundComponent.BoxCompoundShapes.Count + compoundComponent.SphereCompoundShapes.Count;
            using (var compoundBuilder = new CompoundBuilder(bufferPool, simulation.Shapes, compoundCount))
            {
                foreach (var boxCompound in compoundComponent.BoxCompoundShapes)
                {
                    var shape = new Box(boxCompound.Size.X, boxCompound.Size.Y, boxCompound.Size.Z);

                    BodyInertia compoundShapeInertia = default;
                    if (compoundComponent.Interactivity == PhysicsInteractivity.Dynamic)
                        shape.ComputeInertia(1, out compoundShapeInertia);

                    var pose = new RigidPose(boxCompound.RelativeOffset);
                    if (compoundComponent.Interactivity == PhysicsInteractivity.Dynamic)
                        compoundBuilder.Add(shape, pose, 1);
                    else
                        compoundBuilder.AddForKinematic(shape, pose, 1);
                }

                foreach (var sphereCompound in compoundComponent.SphereCompoundShapes)
                {
                    var shape = new Sphere(sphereCompound.Radius);

                    BodyInertia compoundShapeInertia = default;
                    if (compoundComponent.Interactivity == PhysicsInteractivity.Dynamic)
                        shape.ComputeInertia(1, out compoundShapeInertia);

                    var pose = new RigidPose(sphereCompound.RelativeOffset);
                    if (compoundComponent.Interactivity == PhysicsInteractivity.Dynamic)
                        compoundBuilder.Add(shape, pose, 1);
                    else
                        compoundBuilder.AddForKinematic(shape, pose, 1);
                }

                Buffer<CompoundChild> compoundChildren = default;
                if (compoundComponent.Interactivity == PhysicsInteractivity.Dynamic)
                    compoundBuilder.BuildDynamicCompound(out compoundChildren, out inertia/*, out var center*/);
                else
                    compoundBuilder.BuildKinematicCompound(out compoundChildren);

                var compundShape = new BigCompound(compoundChildren, simulation.Shapes, bufferPool);
                shapeIndex = simulation.Shapes.Add(compundShape);
            }
        }

        private void BuildChunkShape(PhysicsChunkComponent chunkComponent, out TypedIndex shapeIndex)
        {
            var voxelShape = new Box(1, 1, 1);

            using (var compoundBuilder = new CompoundBuilder(bufferPool, simulation.Shapes, 1000))
            {
                for (var x = 0; x < Chunk.CHUNK_X_SIZE; x++)
                {
                    for (var y = 0; y < Chunk.CHUNK_Y_SIZE; y++)
                    {
                        for (var z = 0; z < Chunk.CHUNK_Z_SIZE; z++)
                        {
                            if (chunkComponent.Chunk.Blocks[x + (y * Chunk.CHUNK_X_SIZE) + (z * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive)
                            {
                                var anySurroundingBlocksInactive =
                                    (x > 0 ? !chunkComponent.Chunk.Blocks[x - 1 + (y * Chunk.CHUNK_X_SIZE) + (z * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive : true)
                                    || (x < Chunk.CHUNK_X_SIZE - 1 ? !chunkComponent.Chunk.Blocks[x + 1 + (y * Chunk.CHUNK_X_SIZE) + (z * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive : true)
                                    || (y > 0 ? !chunkComponent.Chunk.Blocks[x + ((y - 1) * Chunk.CHUNK_X_SIZE) + (z * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive : true)
                                    || (y < Chunk.CHUNK_Y_SIZE - 1 ? !chunkComponent.Chunk.Blocks[x + ((y + 1) * Chunk.CHUNK_X_SIZE) + (z * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive : true)
                                    || (z > 0 ? !chunkComponent.Chunk.Blocks[x + (y * Chunk.CHUNK_X_SIZE) + ((z - 1) * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive : true)
                                    || (z < Chunk.CHUNK_Z_SIZE - 1 ? !chunkComponent.Chunk.Blocks[x + (y * Chunk.CHUNK_X_SIZE) + ((z + 1) * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)].IsActive : true);
                                if (anySurroundingBlocksInactive)
                                {
                                    var pose = new RigidPose(new Vector3(x, y, z));
                                    compoundBuilder.AddForKinematic(voxelShape, pose, 1);
                                }
                            }
                        }
                    }
                }

                compoundBuilder.BuildKinematicCompound(out var compoundChildren);

                var compundShape = new BigCompound(compoundChildren, simulation.Shapes, bufferPool);
                shapeIndex = simulation.Shapes.Add(compundShape);
            }
        }
    }
}
