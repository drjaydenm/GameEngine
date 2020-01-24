﻿using System;
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

        private BlockWorld world;
        private Chunk previousChunk;
        private Chunk currentChunk;
        private BlockWorldGenerator worldGenerator;
        private bool shouldGenerateChunks = true;
        private bool shouldShowChunkDebug;
        private PriorityQueue<Coord3> coordsToGenerate;
        private HashSet<Coord3> coordsToGenerateSet;
        private List<Entity> boxes;
        private Chunk lookingAtChunk;
        private Block? lookingAtBlock;
        private Coord3 lookingAtBlockCoord;
        private Vector3 rayHitPosition;
        private Entity playerEntity;
        private CharacterController character;
        private bool playerSpawned;
        private bool debugCamera = true;
        private Entity[] movingPlatforms;

        private const int CHUNK_GENERATION_RADIUS = 10;
        private const int CHUNK_GENERATE_PER_FRAME = 10;

        public GameScene()
        {
            coordsToGenerate = new PriorityQueue<Coord3>();
            coordsToGenerateSet = new HashSet<Coord3>();
            boxes = new List<Entity>();
        }

        public override void LoadScene()
        {
            base.LoadScene();

            Renderer.LightDirection = Vector3.Normalize(new Vector3(1, -1, 1));

            world = new BlockWorld(Engine, this, "World");
            AddEntity(world);
            worldGenerator = new BlockWorldGenerator(world);

            var camera = new DebugCamera(new Vector3(8, 80, 8), Vector3.UnitZ, 1, 0.1f, 500, Engine);
            camera.DisableRotation = true;

            playerEntity = new Entity(this, "Player");
            playerEntity.AddComponent(camera);
            AddEntity(playerEntity);
            SetActiveCamera(camera);

            Engine.InputManager.Mouse.IsMouseLocked = true;

            var chunks = worldGenerator.GenerateWorld(CHUNK_GENERATION_RADIUS * 2, CHUNK_GENERATION_RADIUS * 2, CHUNK_GENERATION_RADIUS * 2);

            chunks.ToList().ForEach(c => world.AddChunk(c));

            movingPlatforms = new Entity[4];
            var random = new Random();
            for (var i = 0; i < movingPlatforms.Length; i++)
            {
                var platform = new Entity(this, $"Platform{i + 1}");
                movingPlatforms[i] = platform;
                AddEntity(platform);
                platform.AddComponent(new MovingPlatformController(Engine)
                {
                    PlatformSize = new Vector3(2, 0.1f, 2)
                });
                platform.Transform.Position = new Vector3((float)random.NextDouble() * 10, 50, (float)random.NextDouble() * 10);
            }
        }

        public override void Update()
        {
            base.Update();

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.Escape))
            {
                Engine.Window.Exit();
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.F11))
            {
                Engine.Window.WindowState = Engine.Window.WindowState != WindowState.BorderlessFullScreen ? WindowState.BorderlessFullScreen : WindowState.Normal;
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.F10))
            {
                Engine.Window.WindowState = Engine.Window.WindowState != WindowState.FullScreen ? WindowState.FullScreen : WindowState.Normal;
            }

            if (Engine.GameTimeTotal.TotalMilliseconds > 500 && Engine.InputManager.Mouse.IsMouseLocked)
            {
                ((DebugCamera)ActiveCamera).DisableRotation = false;
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.Tilde))
            {
                Engine.InputManager.Mouse.IsMouseLocked = !Engine.InputManager.Mouse.IsMouseLocked;
                ((DebugCamera)ActiveCamera).DisableRotation = !Engine.InputManager.Mouse.IsMouseLocked;
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.Backslash))
            {
                shouldGenerateChunks = !shouldGenerateChunks;
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.Slash))
            {
                shouldShowChunkDebug = !shouldShowChunkDebug;
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.Enter))
            {
                PhysicsSystem.DebugEnabled = !PhysicsSystem.DebugEnabled;
            }

            if (Engine.InputManager.Keyboard.WasKeyPressed(Keys.F1) && playerSpawned)
            {
                debugCamera = !debugCamera;
                if (debugCamera)
                {
                    playerEntity.RemoveComponent(character);
                }
                else
                {
                    playerEntity.AddComponent(character);
                    playerEntity.Transform.Position = ActiveCamera.Position - (Vector3.UnitY * character.CameraHeightOffset);
                }
            }

            if (Engine.InputManager.Mouse.ScrollDelta != 0)
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

            if (Engine.InputManager.Mouse.WasButtonPressed(MouseButtons.Left) && lookingAtBlock != null)
            {
                lookingAtChunk.SetBlockIsActive(lookingAtBlockCoord.X, lookingAtBlockCoord.Y, lookingAtBlockCoord.Z, false);

                world.UpdateChunk(lookingAtChunk);
            }
            if (Engine.InputManager.Mouse.WasButtonPressed(MouseButtons.Middle) && lookingAtBlock != null)
            {
                BlowUpVoxels(rayHit.Position, 4);
            }
            if (Engine.InputManager.Mouse.WasButtonPressed(MouseButtons.Right) && lookingAtBlock != null)
            {
                var normalAsCoord = NormalToCoord(rayHit.Normal);
                var coordOfBlockToAdd = lookingAtBlockCoord + normalAsCoord;
                var chunkOfBlockToAdd = lookingAtChunk;
                if (coordOfBlockToAdd.X == -1 || coordOfBlockToAdd.X == Chunk.CHUNK_X_SIZE
                    || coordOfBlockToAdd.Y == -1 || coordOfBlockToAdd.Y == Chunk.CHUNK_Y_SIZE
                    || coordOfBlockToAdd.Z == -1 || coordOfBlockToAdd.Z == Chunk.CHUNK_Z_SIZE)
                {
                    chunkOfBlockToAdd = world.FindChunkByOffset(lookingAtChunk, normalAsCoord);
                    coordOfBlockToAdd -= normalAsCoord * new Coord3(Chunk.CHUNK_X_SIZE, Chunk.CHUNK_Y_SIZE, Chunk.CHUNK_Z_SIZE);
                }

                chunkOfBlockToAdd.SetBlockIsActive(coordOfBlockToAdd.X, coordOfBlockToAdd.Y, coordOfBlockToAdd.Z, true);

                world.UpdateChunk(chunkOfBlockToAdd);
            }

            const float lightRotateSpeed = 0.5f;
            Renderer.LightDirection = new Vector3(
                (float)Math.Sin(Engine.GameTimeTotal.TotalSeconds * lightRotateSpeed),
                Renderer.LightDirection.Y,
                (float)Math.Cos(Engine.GameTimeTotal.TotalSeconds * lightRotateSpeed));
            
            currentChunk = world.FindChunkByWorldPosition(ActiveCamera.Position);
            if (currentChunk?.Coordinate != previousChunk?.Coordinate && currentChunk != null && shouldGenerateChunks)
            {
                QueueNewChunksIfRequired(currentChunk);
                UnloadChunksIfRequired(currentChunk);
                world.UpdatePlayerPosition(ActiveCamera.Position);
            }

            if (coordsToGenerate.Count > 0)
            {
                GenerateChunks();
            }

            previousChunk = currentChunk;

            if (!playerSpawned && world.ChunksToUpdateCount <= 0)
            {
                // Spawn the player
                var maxChunkHeight = (float)(world.Chunks.Where(c => c.Chunk.IsAnyBlockActive).Max(c => c.Chunk.Coordinate.Y) + 1) * Chunk.CHUNK_Y_SIZE;
                var playerYOffset = maxChunkHeight;
                var hit = PhysicsSystem.Raycast(new Vector3(8, maxChunkHeight, 8), -Vector3.UnitY, 250, PhysicsInteractivity.Static);
                var playerHeight = 0.9f;
                if (hit.DidHit)
                {
                    playerYOffset = hit.Position.Y + playerHeight;
                }
                character = new CharacterController(Engine, this, new Vector3(8, playerYOffset, 8), playerHeight, 0.4f, 80);
                playerEntity.AddComponent(character);

                playerSpawned = true;
                debugCamera = false;
            }

            for (var i = 0; i < movingPlatforms.Length; i++)
            {
                var controller = movingPlatforms[i].GetComponentsOfType<MovingPlatformController>().FirstOrDefault();
                controller.MovementDirection = Vector3.UnitX * (float)Math.Sin(Engine.GameTimeTotal.TotalSeconds / 2f) * 4f;
            }

            var font = "Fonts/OpenSans-Regular.woff";
            var fontSize = 15;
            var fontColor = RgbaFloat.White;
            var yAccumulated = 0;
            var lineSpacing = 5;
            Engine.TextRenderer.DrawText($"FPS: {Engine.PerformanceCounters.FramesPerSecond} / UPS: {Engine.PerformanceCounters.UpdatesPerSecond}", new Vector2(5, yAccumulated += lineSpacing), fontColor, font, fontSize);
            Engine.TextRenderer.DrawText($"Pos: {playerEntity.Transform.Position}", new Vector2(5, yAccumulated += lineSpacing + fontSize), fontColor, font, fontSize);
            Engine.TextRenderer.DrawText($"Chunks: {ChunkCount} / Gen: {ChunkGenerationQueuedCount} / Upd: {ChunkUpdateQueuedCount}", new Vector2(5, yAccumulated += lineSpacing + fontSize), fontColor, font, fontSize);
        }

        public override void Draw()
        {
            base.Draw();

            var camera = ActiveCamera;
            Engine.DebugGraphics.DrawLine(Vector3.Zero, Vector3.UnitX * 50, RgbaFloat.Red, camera.View * camera.Projection);
            Engine.DebugGraphics.DrawLine(Vector3.Zero, Vector3.UnitY * 50, RgbaFloat.Green, camera.View * camera.Projection);
            Engine.DebugGraphics.DrawLine(Vector3.Zero, Vector3.UnitZ * 50, RgbaFloat.Blue, camera.View * camera.Projection);
            Engine.DebugGraphics.DrawArrow(-Renderer.LightDirection * 100 + camera.Position, -Renderer.LightDirection * 60 + camera.Position, RgbaFloat.Yellow, camera.View * camera.Projection);

            if (shouldShowChunkDebug)
            {
                var cameraInChunk = world.FindChunkByWorldPosition(camera.Position);
                foreach (var chunk in world.Chunks)
                {
                    // Make the cube slightly smaller so we can see all sides of it
                    var scale = cameraInChunk?.Coordinate == chunk.Chunk.Coordinate ? Chunk.CHUNK_X_SIZE * 0.99f : Chunk.CHUNK_X_SIZE;
                    var color = cameraInChunk?.Coordinate == chunk.Chunk.Coordinate ? RgbaFloat.White : RgbaFloat.Pink;

                    var transform = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(chunk.Chunk.WorldPositionCentroid);
                    Engine.DebugGraphics.DrawCube(color, transform * camera.View * camera.Projection);
                }
            }

            // Draw a debug cube around the block we are currently looking at
            if (lookingAtChunk != null && lookingAtBlock != null)
            {
                var blockPosition = new Vector3(lookingAtBlockCoord.X, lookingAtBlockCoord.Y, lookingAtBlockCoord.Z) + lookingAtChunk.WorldPosition + new Vector3(0.5f);
                var transform = Matrix4x4.CreateTranslation(blockPosition);
                Engine.DebugGraphics.DrawCube(RgbaFloat.Orange, transform * camera.View * camera.Projection);
            }
            var rayHitTransform = Matrix4x4.CreateScale(0.05f) * Matrix4x4.CreateTranslation(rayHitPosition);
            Engine.DebugGraphics.DrawCube(RgbaFloat.Black, rayHitTransform * camera.View * camera.Projection);
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
                        if (!chunkExists && !coordsToGenerateSet.Contains(coord))
                        {
                            var dist = ActiveCamera.Position - coord;
                            var distanceFromPlayer = (int)Math.Abs(Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y + dist.Z * dist.Z));
                            coordsToGenerate.Enqueue(coord, distanceFromPlayer);
                            coordsToGenerateSet.Add(coord);
                        }
                    }
                }
            }
        }

        private void UnloadChunksIfRequired(Chunk newChunk)
        {
            foreach (var chunk in world.Chunks)
            {
                if (chunk.Chunk.Coordinate.X < newChunk.Coordinate.X - CHUNK_GENERATION_RADIUS - 1 || chunk.Chunk.Coordinate.X > newChunk.Coordinate.X + CHUNK_GENERATION_RADIUS + 1
                    || chunk.Chunk.Coordinate.Y < newChunk.Coordinate.Y - CHUNK_GENERATION_RADIUS - 1 || chunk.Chunk.Coordinate.Y > newChunk.Coordinate.Y + CHUNK_GENERATION_RADIUS + 1
                    || chunk.Chunk.Coordinate.Z < newChunk.Coordinate.Z - CHUNK_GENERATION_RADIUS - 1 || chunk.Chunk.Coordinate.Z > newChunk.Coordinate.Z + CHUNK_GENERATION_RADIUS + 1)
                {
                    world.RemoveChunk(chunk.Chunk);
                }
            }
        }

        private void GenerateChunks()
        {
            var chunksAlreadyGenerated = 0;
            while (chunksAlreadyGenerated < CHUNK_GENERATE_PER_FRAME && coordsToGenerate.Count > 0)
            {
                var coord = coordsToGenerate.Dequeue();
                coordsToGenerateSet.Remove(coord);
                world.AddChunk(worldGenerator.GenerateChunk(coord.X, coord.Y, coord.Z));
                chunksAlreadyGenerated++;
            }
        }

        private Entity ShootBox()
        {
            var box = new Entity(this, "Box");
            box.Transform.Position = ActiveCamera.Position + (ActiveCamera.ViewDirection * 1.5f);

            var physicsBox = new PhysicsBoxComponent(new Vector3(0.5f), PhysicsInteractivity.Dynamic)
            {
                Mass = 1
            };
            box.AddComponent(physicsBox);
            physicsBox.LinearVelocity = ActiveCamera.ViewDirection * 20;

            var boxRenderable = new BasicRenderable<VertexPositionNormalMaterial>(Engine, new Material(Engine, ShaderCode.VertexCode, ShaderCode.FragmentCode));
            var vertices = ShapeBuilder.BuildCubeVertices().Select(v => new VertexPositionNormalMaterial(v, Vector3.UnitY, 1)).ToArray();
            var indices = ShapeBuilder.BuildCubeIndicies();
            var mesh = new Mesh<VertexPositionNormalMaterial>(vertices, indices);
            boxRenderable.SetMesh(VertexPositionNormalMaterial.VertexLayoutDescription, mesh);
            box.AddComponent(boxRenderable);

            AddEntity(box);

            return box;
        }

        private Coord3 NormalToCoord(Vector3 normal)
        {
            if (normal.X > 0 && normal.X > normal.Y && normal.X > normal.Z)
                return Coord3.UnitX;
            else if (normal.X <= 0 && normal.X < normal.Y && normal.X < normal.Z)
                return -Coord3.UnitX;
            else if (normal.Y > 0 && normal.Y > normal.X && normal.Y > normal.Z)
                return Coord3.UnitY;
            else if (normal.Y <= 0 && normal.Y < normal.X && normal.Y < normal.Z)
                return -Coord3.UnitY;
            else if (normal.Z > 0 && normal.Z > normal.X && normal.Z > normal.Y)
                return Coord3.UnitZ;
            else if (normal.Z <= 0 && normal.Z < normal.X && normal.Z < normal.Y)
                return -Coord3.UnitZ;
            else
                return Coord3.UnitX;
        }

        private void BlowUpVoxels(Vector3 explosionOrigin, int radius)
        {
            var updatedChunks = new HashSet<Chunk>();
            var originChunk = world.FindChunkByWorldPosition(explosionOrigin);
            if (originChunk == null)
                return;

            var originBlockCoord = world.ConvertWorldPositionToBlockCoordinate(explosionOrigin);
            var originBlockCentroid = originChunk.WorldPosition + originBlockCoord + new Vector3(0.5f);

            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var z = -radius; z <= radius; z++)
                    {
                        var dist = (originBlockCentroid - new Vector3(originBlockCentroid.X + x, originBlockCentroid.Y + y, originBlockCentroid.Z + z)).Length();
                        if (dist > radius)
                            continue;

                        var currentChunkOffset = new Coord3(
                            (int)Math.Floor((float)(originBlockCoord.X + x) / Chunk.CHUNK_X_SIZE),
                            (int)Math.Floor((float)(originBlockCoord.Y + y) / Chunk.CHUNK_Y_SIZE),
                            (int)Math.Floor((float)(originBlockCoord.Z + z) / Chunk.CHUNK_Z_SIZE));
                        var currentChunk = originChunk;
                        if (currentChunkOffset != Coord3.Zero)
                        {
                            currentChunk = world.FindChunkByOffset(originChunk, currentChunkOffset);
                            if (currentChunk == null)
                                continue;
                        }

                        var currentBlockCoord = new Vector3(originBlockCoord.X + x, originBlockCoord.Y + y, originBlockCoord.Z + z);
                        currentBlockCoord += Chunk.CHUNK_SIZE * -currentChunkOffset;
                        currentChunk.SetBlockIsActive((int)currentBlockCoord.X, (int)currentBlockCoord.Y, (int)currentBlockCoord.Z,
                            false);
                        updatedChunks.Add(currentChunk);
                    }
                }
            }

            foreach (var chunk in updatedChunks)
            {
                world.UpdateChunk(chunk);
            }
        }
    }
}
