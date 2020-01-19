using System;
using System.Collections.Generic;
using System.Numerics;
using GameEngine.Core.Entities;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.World
{
    public class BlockWorld : Entity
    {
        public IEnumerable<LoadedChunk> Chunks => chunks.Values;
        public int ChunksToUpdateCount => chunksToUpdate.Count;
        public Vector3 PlayerPosition { get; private set; }
        public Coord3 PlayerInChunkCoord { get; private set; }

        private readonly Engine engine;
        private readonly Dictionary<Coord3, LoadedChunk> chunks;
        private readonly PriorityQueue<Coord3> chunksToUpdate;
        private readonly Queue<Coord3> chunksToRemove;
        private readonly Material material;

        private const int CHUNKS_UPDATE_PER_FRAME = 10;
        private const int CHUNKS_REMOVE_PER_FRAME = 10;

        public BlockWorld(Engine engine, Scene scene, string name) : base(scene, name)
        {
            this.engine = engine;

            chunks = new Dictionary<Coord3, LoadedChunk>();
            chunksToUpdate = new PriorityQueue<Coord3>();
            chunksToRemove = new Queue<Coord3>();
            material = new Material(engine, ShaderCode.VertexCode, ShaderCode.FragmentCode);
        }

        public override void Update()
        {
            base.Update();

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
            if (chunksToRemove.Count > 0)
            {
                RemoveChunks();
            }
        }

        private void UpdateChunks()
        {
            var chunksUpdatedThisFrame = 0;
            while (chunksUpdatedThisFrame < CHUNKS_UPDATE_PER_FRAME && chunksToUpdate.Count > 0)
            {
                var chunkCoord = chunksToUpdate.Dequeue();

                // Check if this chunk has since been removed
                if (!chunks.ContainsKey(chunkCoord))
                    continue;

                AddOrUpdateRenderable(chunkCoord);
                AddOrUpdatePhysics(chunkCoord);

                chunksUpdatedThisFrame++;
            }
        }

        private void RemoveChunks()
        {
            var chunksRemovedThisFrame = 0;
            while (chunksRemovedThisFrame < CHUNKS_REMOVE_PER_FRAME && chunksToRemove.Count > 0)
            {
                var chunkCoord = chunksToRemove.Dequeue();
                var chunk = chunks[chunkCoord];
                if (chunk.Renderable != null)
                {
                    RemoveComponent(chunk.Renderable);
                    chunk.Renderable.Dispose();
                }
                if (chunk.Physics != null)
                {
                    RemoveComponent(chunk.Physics);
                }
                chunks.Remove(chunkCoord);
            }
        }

        public void AddChunk(Chunk chunk)
        {
            if (chunks.ContainsKey(chunk.Coordinate))
                return;

            chunks.Add(chunk.Coordinate, new LoadedChunk(chunk));
            QueueCoordForUpdate(chunk.Coordinate);

            QueueSurroundingChunksForUpdate(chunk);
        }

        public void RemoveChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate))
                return;

            chunksToRemove.Enqueue(chunk.Coordinate);
        }

        public void UpdateChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate))
                return;

            QueueCoordForUpdate(chunk.Coordinate);

            QueueSurroundingChunksForUpdate(chunk);
        }

        public Chunk FindChunkByOffset(Chunk chunk, Coord3 offset)
        {
            var targetOffset = chunk.Coordinate + offset;

            return FindChunkByCoordinate(targetOffset);
        }

        public Chunk FindChunkByCoordinate(Coord3 coordinate)
        {
            if (!chunks.ContainsKey(coordinate))
                return null;

            return chunks[coordinate].Chunk;
        }

        public Chunk FindChunkByWorldPosition(Vector3 position)
        {
            var chunkPosition = ConvertWorldPositionToChunkCoordinate(position);

            return FindChunkByCoordinate(chunkPosition);
        }

        public Block? FindBlockByWorldPosition(Vector3 position)
        {
            var chunk = FindChunkByWorldPosition(position);
            if (chunk == null)
                return null;

            var flooredPosition = new Vector3((float)Math.Floor(position.X), (float)Math.Floor(position.Y), (float)Math.Floor(position.Z));
            var blockIndex = flooredPosition - (Chunk.CHUNK_SIZE * chunk.Coordinate);
            return chunk.Blocks[(int)blockIndex.X, (int)blockIndex.Y, (int)blockIndex.Z];
        }

        public Coord3 ConvertWorldPositionToChunkCoordinate(Vector3 position)
        {
            var worldToChunk = position / Chunk.CHUNK_SIZE;

            return new Coord3(
                (int)Math.Floor(worldToChunk.X),
                (int)Math.Floor(worldToChunk.Y),
                (int)Math.Floor(worldToChunk.Z));
        }

        public Coord3 ConvertWorldPositionToBlockCoordinate(Vector3 position)
        {
            var chunkPosition = ConvertWorldPositionToChunkCoordinate(position);

            var blockIndex = position - (Chunk.CHUNK_SIZE * chunkPosition);

            return new Coord3(
                (int)Math.Floor(blockIndex.X),
                (int)Math.Floor(blockIndex.Y),
                (int)Math.Floor(blockIndex.Z));
        }

        public void UpdatePlayerPosition(Vector3 newPosition)
        {
            PlayerPosition = newPosition;

            var currentChunk = FindChunkByWorldPosition(PlayerPosition);
            if (currentChunk != null)
                PlayerInChunkCoord = currentChunk.Coordinate;
            else
                PlayerInChunkCoord = Coord3.Zero;
        }

        private void QueueCoordForUpdate(Coord3 chunkCoord)
        {
            var dist = PlayerInChunkCoord - chunkCoord;
            var distanceFromPlayer = (int)Math.Abs(Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y + dist.Z * dist.Z));
            chunksToUpdate.Enqueue(chunkCoord, distanceFromPlayer);
        }

        private void AddOrUpdateRenderable(Coord3 chunkCoord)
        {
            var loadedChunk = chunks[chunkCoord];
            var chunk = loadedChunk.Chunk;
            var chunkShouldRender = chunk.IsAnyBlockActive;

            if (!chunkShouldRender && loadedChunk.Renderable != null)
            {
                RemoveComponent(loadedChunk.Renderable);
                loadedChunk.Renderable.Dispose();

                loadedChunk.Renderable = null;
            }
            else
            {
                var mesh = ChunkMeshGenerator.GenerateMesh(chunk, this);
                if (loadedChunk.Renderable != null)
                {
                    loadedChunk.Renderable.UpdateChunk(mesh);
                    if (mesh == null)
                    {
                        RemoveComponent(loadedChunk.Renderable);
                        loadedChunk.Renderable.Dispose();

                        loadedChunk.Renderable = null;
                    }
                }
                else
                {
                    if (mesh != null)
                    {
                        var renderable = new ChunkRenderable(chunk, engine, material);
                        renderable.UpdateChunk(mesh);

                        loadedChunk.Renderable = renderable;

                        AddComponent(renderable);
                    }
                }
            }
        }

        private void AddOrUpdatePhysics(Coord3 chunkCoord)
        {
            var loadedChunk = chunks[chunkCoord];
            var chunk = loadedChunk.Chunk;
            var chunkShouldHavePhysics = chunk.IsAnyBlockActive;

            if (loadedChunk.Physics != null && !chunkShouldHavePhysics)
            {
                RemoveComponent(loadedChunk.Physics);

                loadedChunk.Physics = null;
            }
            else if (chunkShouldHavePhysics)
            {
                // If we already have a physics object, remove it
                if (loadedChunk.Physics != null)
                {
                    RemoveComponent(loadedChunk.Physics);

                    loadedChunk.Physics = null;
                }

                PhysicsComponent physics;
                var isChunkSolid = !chunk.IsAnyBlockInactive;

                // Use a simple box for the whole chunk if possible
                if (isChunkSolid)
                {
                    physics = new PhysicsBoxComponent(Chunk.CHUNK_SIZE, PhysicsInteractivity.Static)
                    {
                        PositionOffset = chunk.WorldPositionCentroid
                    };
                }
                else
                {
                    var compound = new PhysicsCompoundComponent(PhysicsInteractivity.Static)
                    {
                        PositionOffset = chunk.WorldPosition + (Chunk.CHUNK_SIZE * Chunk.CHUNK_BLOCK_RATIO * 0.5f)
                    };
                    for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
                    {
                        for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
                        {
                            for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
                            {
                                if (chunk.Blocks[x, y, z].IsActive)
                                {
                                    var anySurroundingBlocksInactive =
                                        (x > 0 ? !chunk.Blocks[x - 1, y, z].IsActive : true)
                                        || (x < Chunk.CHUNK_X_SIZE - 1 ? !chunk.Blocks[x + 1, y, z].IsActive : true)
                                        || (y > 0 ? !chunk.Blocks[x, y - 1, z].IsActive : true)
                                        || (y < Chunk.CHUNK_Y_SIZE - 1 ? !chunk.Blocks[x, y + 1, z].IsActive : true)
                                        || (z > 0 ? !chunk.Blocks[x, y, z - 1].IsActive : true)
                                        || (z < Chunk.CHUNK_Z_SIZE - 1 ? !chunk.Blocks[x, y, z + 1].IsActive : true);
                                    if (anySurroundingBlocksInactive)
                                    {
                                        compound.AddBox(new Vector3(x, y, z), Vector3.One);
                                    }
                                }
                            }
                        }
                    }
                    physics = compound;
                }

                loadedChunk.Physics = physics;

                AddComponent(physics);
            }
        }

        private void QueueSurroundingChunksForUpdate(Chunk chunk)
        {
            void QueueChunkForUpdateIfExists(Coord3 coordOffset)
            {
                var chunkAbove = FindChunkByOffset(chunk, coordOffset);
                if (chunkAbove == null)
                    return;

                if (!chunksToUpdate.Contains(chunkAbove.Coordinate))
                    QueueCoordForUpdate(chunkAbove.Coordinate);
            }

            // Update surrounding chunks if not queued for update already
            QueueChunkForUpdateIfExists(Coord3.UnitX);
            QueueChunkForUpdateIfExists(Coord3.UnitY);
            QueueChunkForUpdateIfExists(Coord3.UnitZ);
            QueueChunkForUpdateIfExists(-Coord3.UnitX);
            QueueChunkForUpdateIfExists(-Coord3.UnitY);
            QueueChunkForUpdateIfExists(-Coord3.UnitZ);
        }
    }
}
