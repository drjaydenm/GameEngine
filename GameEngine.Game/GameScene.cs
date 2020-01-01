using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using GameEngine.Core;
using GameEngine.Core.Camera;
using GameEngine.Core.Entities;
using GameEngine.Core.Graphics;
using GameEngine.Core.Input;
using GameEngine.Core.World;

namespace GameEngine.Game
{
    public class GameScene : Scene
    {
        public int ChunkCount => world.Chunks.Count();
        public int ChunkGenerationQueuedCount => coordsToGenerate.Count;
        public int ChunkUpdateQueuedCount => world.ChunksToUpdateCount;

        private readonly Engine engine;
        private BlockWorld world;
        private Chunk previousChunk;
        private Chunk currentChunk;
        private BlockWorldGenerator worldGenerator;
        private bool shouldGenerateChunks = true;
        private bool shouldShowChunkDebug;
        private Queue<Coord3> coordsToGenerate;
        private List<Entity> boxes;
        private Chunk lookingAtChunk;
        private Block? lookingAtBlock;
        private Coord3 lookingAtBlockCoord;
        private Vector3 rayHitPosition;
        private Entity playerEntity;
        private CharacterController character;
        private bool playerSpawned;
        private bool debugCamera = true;

        private const int CHUNK_GENERATION_RADIUS = 10;
        private const int CHUNK_GENERATE_PER_FRAME = 10;

        public GameScene(Engine engine)
        {
            this.engine = engine;

            worldGenerator = new BlockWorldGenerator();
            coordsToGenerate = new Queue<Coord3>();
            boxes = new List<Entity>();
        }

        public override void LoadScene()
        {
            base.LoadScene();

            Renderer.LightDirection = Vector3.Normalize(new Vector3(1, -1, 1));

            world = new BlockWorld(engine, this, "World");
            AddEntity(world);

            var camera = new DebugCamera(Vector3.Zero, Vector3.UnitZ, 1, 0.5f, 500, engine);
            camera.DisableRotation = true;

            playerEntity = new Entity(this, "Player");
            playerEntity.AddComponent(camera);
            AddEntity(playerEntity);
            SetActiveCamera(camera);

            engine.InputManager.Mouse.IsMouseLocked = true;

            var chunks = worldGenerator.GenerateWorld(CHUNK_GENERATION_RADIUS * 2, CHUNK_GENERATION_RADIUS * 2, CHUNK_GENERATION_RADIUS * 2);

            chunks.ToList().ForEach(c => world.AddChunk(c));
        }

        public override void Update()
        {
            base.Update();

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.Escape))
            {
                engine.Window.Exit();
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.F11))
            {
                engine.Window.WindowState = engine.Window.WindowState != WindowState.BorderlessFullScreen ? WindowState.BorderlessFullScreen : WindowState.Normal;
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.F10))
            {
                engine.Window.WindowState = engine.Window.WindowState != WindowState.FullScreen ? WindowState.FullScreen : WindowState.Normal;
            }

            if (engine.GameTimeTotal.TotalMilliseconds > 500 && engine.InputManager.Mouse.IsMouseLocked)
            {
                ((DebugCamera)ActiveCamera).DisableRotation = false;
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.Tilde))
            {
                engine.InputManager.Mouse.IsMouseLocked = !engine.InputManager.Mouse.IsMouseLocked;
                ((DebugCamera)ActiveCamera).DisableRotation = !engine.InputManager.Mouse.IsMouseLocked;
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.Backslash))
            {
                shouldGenerateChunks = !shouldGenerateChunks;
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.Slash))
            {
                shouldShowChunkDebug = !shouldShowChunkDebug;
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.Enter))
            {
                PhysicsSystem.DebugEnabled = !PhysicsSystem.DebugEnabled;
            }

            if (engine.InputManager.Keyboard.WasKeyPressed(Keys.F1))
            {
                debugCamera = !debugCamera;
                if (debugCamera)
                {
                    playerEntity.RemoveComponent(character);
                }
                else
                {
                    playerEntity.AddComponent(character);
                }
            }

            if (engine.InputManager.Mouse.IsButtonDown(MouseButtons.Left))
            {
                var box = ShootBox();
                boxes.Add(box);
            }

            foreach (var box in boxes)
            {
                var renderComponent = box.GetComponentsOfType<BasicRenderable<VertexPositionNormalMaterial>>().First();
                renderComponent.SetWorldTransform(Matrix4x4.CreateFromQuaternion(box.Transform.Rotation) * Matrix4x4.CreateScale(0.5f) * Matrix4x4.CreateTranslation(box.Transform.Position));
            }

            // Check which block we are looking at
            var rayHit = PhysicsSystem.Raycast(ActiveCamera.Position + (ActiveCamera.ViewDirection * 0.6f), ActiveCamera.ViewDirection, 1000,
                PhysicsInteractivity.Static | PhysicsInteractivity.Dynamic | PhysicsInteractivity.Kinematic,
                character != null ? new[] { character.PhysicsComponent } : null);
            if (rayHit.DidHit)
            {
                rayHitPosition = rayHit.Position;
                // Trace along the inverse of the normal a little so we are slightly inside the block
                var adjustedPosition = rayHit.Position - (rayHit.Normal * 0.05f);

                lookingAtChunk = world.FindChunkByWorldPosition(adjustedPosition);
                lookingAtBlock = world.FindBlockByWorldPosition(adjustedPosition);
                if (lookingAtBlock != null && lookingAtBlock.Value.IsActive)
                {
                    
                    lookingAtBlockCoord = world.ConvertWorldPositionToBlockCoordinate(adjustedPosition);
                }
                else
                {
                    lookingAtChunk = null;
                    lookingAtBlock = null;
                    lookingAtBlockCoord = Coord3.Zero;
                }
            }
            else
            {
                lookingAtChunk = null;
                lookingAtBlock = null;
                lookingAtBlockCoord = Coord3.Zero;
                rayHitPosition = Vector3.Zero;
            }

            const float lightRotateSpeed = 0.5f;
            Renderer.LightDirection = new Vector3(
                (float)Math.Sin(engine.GameTimeTotal.TotalSeconds * lightRotateSpeed),
                Renderer.LightDirection.Y,
                (float)Math.Cos(engine.GameTimeTotal.TotalSeconds * lightRotateSpeed));
            
            currentChunk = world.FindChunkByWorldPosition(ActiveCamera.Position);
            if (currentChunk != previousChunk && currentChunk != null && shouldGenerateChunks)
            {
                QueueNewChunksIfRequired(currentChunk);
                UnloadChunksIfRequired(currentChunk);
            }

            if (coordsToGenerate.Count > 0)
            {
                GenerateChunks();
            }

            previousChunk = currentChunk;

            if (!playerSpawned && world.ChunksToUpdateCount <= 0)
            {
                // Spawn the player
                var maxChunkHeight = (float)(world.Chunks.Where(c => c.IsAnyBlockActive()).Max(c => c.Coordinate.Y) + 1) * Chunk.CHUNK_Y_SIZE;
                var playerYOffset = maxChunkHeight;
                var hit = PhysicsSystem.Raycast(new Vector3(0, maxChunkHeight, 0), -Vector3.UnitY, 250, PhysicsInteractivity.Static);
                if (hit.DidHit)
                {
                    playerYOffset = hit.Position.Y + 1.8f;
                }
                character = new CharacterController(engine, new Vector3(0, playerYOffset, 0), 1.8f, 0.5f, 80);
                playerEntity.AddComponent(character);

                playerSpawned = true;
                debugCamera = false;
            }
        }

        public override void Draw()
        {
            base.Draw();

            var camera = ActiveCamera;
            engine.DebugGraphics.DrawLine(Vector3.Zero, Vector3.UnitX * 50, RgbaFloat.Red, camera.View * camera.Projection);
            engine.DebugGraphics.DrawLine(Vector3.Zero, Vector3.UnitY * 50, RgbaFloat.Green, camera.View * camera.Projection);
            engine.DebugGraphics.DrawLine(Vector3.Zero, Vector3.UnitZ * 50, RgbaFloat.Blue, camera.View * camera.Projection);
            engine.DebugGraphics.DrawArrow(-Renderer.LightDirection * 100 + camera.Position, -Renderer.LightDirection * 60 + camera.Position, RgbaFloat.Yellow, camera.View * camera.Projection);

            if (shouldShowChunkDebug)
            {
                var cameraInChunk = world.FindChunkByWorldPosition(camera.Position);
                foreach (var chunk in world.Chunks)
                {
                    // Make the cube slightly smaller so we can see all sides of it
                    var scale = cameraInChunk == chunk ? Chunk.CHUNK_X_SIZE * 0.99f : Chunk.CHUNK_X_SIZE;
                    var color = cameraInChunk == chunk ? RgbaFloat.White : RgbaFloat.Pink;

                    var transform = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(chunk.WorldPositionCentroid);
                    engine.DebugGraphics.DrawCube(color, transform * camera.View * camera.Projection);
                }
            }

            // Draw a debug cube around the block we are currently looking at
            if (lookingAtBlock != null)
            {
                var blockPosition = new Vector3(lookingAtBlockCoord.X, lookingAtBlockCoord.Y, lookingAtBlockCoord.Z) + lookingAtChunk.WorldPosition + new Vector3(0.5f);
                var transform = Matrix4x4.CreateTranslation(blockPosition);
                engine.DebugGraphics.DrawCube(RgbaFloat.Orange, transform * camera.View * camera.Projection);
            }
            var rayHitTransform = Matrix4x4.CreateScale(0.05f) * Matrix4x4.CreateTranslation(rayHitPosition);
            engine.DebugGraphics.DrawCube(RgbaFloat.Black, rayHitTransform * camera.View * camera.Projection);
        }

        private void QueueNewChunksIfRequired(Chunk newChunk)
        {
            for (var x = newChunk.Coordinate.X - CHUNK_GENERATION_RADIUS; x <= newChunk.Coordinate.X + CHUNK_GENERATION_RADIUS; x++)
            {
                for (var y = newChunk.Coordinate.Y - CHUNK_GENERATION_RADIUS; y <= newChunk.Coordinate.Y + CHUNK_GENERATION_RADIUS; y++)
                {
                    for (var z = newChunk.Coordinate.Z - CHUNK_GENERATION_RADIUS; z <= newChunk.Coordinate.Z + CHUNK_GENERATION_RADIUS; z++)
                    {
                        var coord = new Coord3(x, y, z);
                        var chunkExists = world.FindChunkByCoordinate(coord) != null;
                        if (!chunkExists && !coordsToGenerate.Contains(coord))
                        {
                            coordsToGenerate.Enqueue(coord);
                        }
                    }
                }
            }
        }

        private void UnloadChunksIfRequired(Chunk newChunk)
        {
            foreach (var chunk in world.Chunks)
            {
                if (chunk.Coordinate.X < newChunk.Coordinate.X - CHUNK_GENERATION_RADIUS - 1 || chunk.Coordinate.X > newChunk.Coordinate.X + CHUNK_GENERATION_RADIUS + 1
                    || chunk.Coordinate.Y < newChunk.Coordinate.Y - CHUNK_GENERATION_RADIUS - 1 || chunk.Coordinate.Y > newChunk.Coordinate.Y + CHUNK_GENERATION_RADIUS + 1
                    || chunk.Coordinate.Z < newChunk.Coordinate.Z - CHUNK_GENERATION_RADIUS - 1 || chunk.Coordinate.Z > newChunk.Coordinate.Z + CHUNK_GENERATION_RADIUS + 1)
                {
                    world.RemoveChunk(chunk);
                }
            }
        }

        private void GenerateChunks()
        {
            var chunksAlreadyGenerated = 0;
            while (chunksAlreadyGenerated < CHUNK_GENERATE_PER_FRAME && coordsToGenerate.TryDequeue(out var coord))
            {
                world.AddChunk(worldGenerator.GenerateChunk(coord.X, coord.Y, coord.Z));
                chunksAlreadyGenerated++;
            }
        }

        private Entity ShootBox()
        {
            var box = new Entity(this, "Box");
            box.Transform.Position = ActiveCamera.Position;

            var physicsBox = new PhysicsBoxComponent(new Vector3(0.5f), PhysicsInteractivity.Dynamic)
            {
                Mass = 1
            };
            box.AddComponent(physicsBox);
            physicsBox.LinearVelocity = ActiveCamera.ViewDirection * 20;

            var boxRenderable = new BasicRenderable<VertexPositionNormalMaterial>(engine, new Material(engine, ShaderCode.VertexCode, ShaderCode.FragmentCode));
            var vertices = ShapeBuilder.BuildCubeVertices().Select(v => new VertexPositionNormalMaterial(v, Vector3.UnitY, 1)).ToArray();
            var indices = ShapeBuilder.BuildCubeIndicies();
            var mesh = new Mesh<VertexPositionNormalMaterial>(vertices, indices);
            boxRenderable.SetMesh(VertexPositionNormalMaterial.VertexLayoutDescription, mesh);
            box.AddComponent(boxRenderable);

            AddEntity(box);

            return box;
        }
    }
}
