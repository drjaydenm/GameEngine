using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Veldrid;
using GameEngine.Core.Entities;
using BepuMesh = BepuPhysics.Collidables.Mesh;
using BepuUtilities;

namespace GameEngine.Core.Physics.BepuPhysics
{
    public class BepuPhysicsSystem : IPhysicsSystem, IDisposable
    {
        public bool DebugEnabled { get; set; }

        private readonly Engine engine;
        private readonly Scene scene;
        private BufferPool bufferPool;
        private Simulation simulation;
        private DefaultThreadDispatcher threadDispatcher;
        private ConcurrentDictionary<PhysicsComponent, BepuPhysicsBody> registeredComponents;

        public BepuPhysicsSystem(Engine engine, Scene scene, Vector3 gravity)
        {
            this.engine = engine;
            this.scene = scene;

            bufferPool = new BufferPool();
            threadDispatcher = new DefaultThreadDispatcher(Environment.ProcessorCount);
            simulation = Simulation.Create(bufferPool, new DefaultNarrowPhaseCallbacks(), new DefaultPoseIntegratorCallbacks(gravity));
            registeredComponents = new ConcurrentDictionary<PhysicsComponent, BepuPhysicsBody>();
        }

        public void DeregisterComponent(PhysicsComponent component)
        {
            if (registeredComponents.ContainsKey(component))
            {
                var body = registeredComponents[component];
                registeredComponents.TryRemove(component, out _);

                simulation.Shapes.Remove(body.ShapeIndex);
                if (component.Interactivity == PhysicsInteractivity.Static)
                {
                    simulation.Statics.Remove(body.StaticReference.Handle);
                }
                else
                {
                    simulation.Bodies.Remove(body.BodyReference.Handle);
                }
            }
        }

        public void RegisterComponent(Entity entity, PhysicsComponent component)
        {
            BepuPhysicsBody body;
            int bodyHandle = -1;
            int staticHandle = -1;
            var shapeIndex = new TypedIndex();
            var inertia = new BodyInertia();

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
                using (var compoundBuilder = new CompoundBuilder(bufferPool, simulation.Shapes, 8))
                {
                    foreach (var compound in compoundComponent.CompoundShapes)
                    {
                        if (compound is BoxCompoundShape boxCompound)
                        {
                            var shape = new Box(boxCompound.Size.X, boxCompound.Size.Y, boxCompound.Size.Z);
                            var pose = new RigidPose(boxCompound.RelativeOffset);
                            compoundBuilder.Add(shape, pose, 1);
                        }
                        else if (compound is SphereCompoundShape sphereCompound)
                        {
                            var shape = new Sphere(sphereCompound.Radius);
                            var pose = new RigidPose(sphereCompound.RelativeOffset);
                            compoundBuilder.Add(shape, pose, 1);
                        }
                    }
                    
                    compoundBuilder.BuildDynamicCompound(out var compoundChildren, out inertia/*, out var center*/);

                    var compundShape = new BigCompound(compoundChildren, simulation.Shapes, bufferPool);
                    shapeIndex = simulation.Shapes.Add(compundShape);
                }
            }
            else if (component is PhysicsMeshComponent meshComponent)
            {
                var meshTriangles = meshComponent.Mesh.TransformAsTriangles();
                bufferPool.Take<Triangle>(meshTriangles.Length, out var triangles);

                for (int i = 0; i < meshTriangles.Length; ++i)
                {
                    triangles[i] = new Triangle(meshTriangles[i].Vertex1, meshTriangles[i].Vertex2, meshTriangles[i].Vertex3);
                }

                var mesh = new BepuMesh(triangles, Vector3.One, bufferPool);
                shapeIndex = simulation.Shapes.Add(mesh);
            }

            if (component.FreezeRotation)
            {
                inertia.InverseInertiaTensor = new Symmetric3x3();
            }

            var collidable = new CollidableDescription(shapeIndex, 0.1f);
            var activity = new BodyActivityDescription(0.01f);

            if (component.Interactivity == PhysicsInteractivity.Dynamic)
            {
                var bodyDescription = BodyDescription.CreateDynamic(
                    entity.Transform.Position + component.PositionOffset, inertia, collidable, activity);
                bodyHandle = simulation.Bodies.Add(bodyDescription);
            }
            else if (component.Interactivity == PhysicsInteractivity.Kinematic)
            {
                var bodyDescription = BodyDescription.CreateKinematic(
                    entity.Transform.Position + component.PositionOffset, collidable, activity);
                bodyHandle = simulation.Bodies.Add(bodyDescription);
            }
            else if (component.Interactivity == PhysicsInteractivity.Static)
            {
                var staticDescription = new StaticDescription(entity.Transform.Position + component.PositionOffset, collidable);
                staticHandle = simulation.Statics.Add(staticDescription);
            }

            body = new BepuPhysicsBody(simulation, bodyHandle != -1);
            body.ShapeIndex = shapeIndex;

            if (bodyHandle != -1)
                body.BodyReference = new BodyReference(bodyHandle, simulation.Bodies);
            else if (staticHandle != -1)
                body.StaticReference = new StaticReference(staticHandle, simulation.Statics);

            component.Body = body;
            registeredComponents.TryAdd(component, body);
        }

        public void Update()
        {
            simulation.Timestep((float)engine.GameTimeElapsed.TotalSeconds, threadDispatcher);
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
                    engine.DebugGraphics.DrawCube(RgbaFloat.Red, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
                else if (kvp.Key is PhysicsCapsuleComponent capsuleComponent)
                {
                    var transform = Matrix4x4.CreateScale(capsuleComponent.Radius, capsuleComponent.Length, capsuleComponent.Radius) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(RgbaFloat.Orange, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
                else if (kvp.Key is PhysicsMeshComponent meshComponent)
                {
                    // TODO debug draw the actual mesh, for now just shows a 1x1x1 cube
                    var transform = Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(RgbaFloat.Blue, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
                else if (kvp.Key is PhysicsCompoundComponent compoundComponent)
                {
                    // TODO debug draw the actual compound shape, for now just shows a 1x1x1 cube
                    /*foreach (var shape in compoundComponent.CompoundShapes)
                    {
                        var transform = Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position + shape.RelativeOffset);
                        engine.DebugGraphics.DrawCube(RgbaFloat.Cyan, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                    }*/

                    var transform = Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
                    engine.DebugGraphics.DrawCube(RgbaFloat.Cyan, transform * scene.ActiveCamera.View * scene.ActiveCamera.Projection);
                }
            }
        }

        public RayHit Raycast(Vector3 origin, Vector3 direction, float maxDistance, PhysicsInteractivity interactivity)
        {
            var hitHandler = new DefaultRayHitHandler(interactivity);
            simulation.RayCast(origin, direction, maxDistance, ref hitHandler);

            PhysicsComponent component = null;
            if (hitHandler.Hit)
            {
                if (hitHandler.Collidable.Mobility == CollidableMobility.Dynamic
                    || hitHandler.Collidable.Mobility == CollidableMobility.Kinematic)
                {
                    component = registeredComponents.FirstOrDefault(b => b.Value.BodyReference.Handle == hitHandler.Collidable.Handle).Key;
                }
                else
                {
                    component = registeredComponents.FirstOrDefault(b => b.Value.StaticReference.Handle == hitHandler.Collidable.Handle).Key;
                }
            }

            return new RayHit(component?.Entity, component, hitHandler.T, hitHandler.Position, hitHandler.Normal, hitHandler.Hit);
        }

        public void Dispose()
        {
            simulation.Dispose();
            bufferPool.Clear();
            threadDispatcher.Dispose();
        }
    }
}
