using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<Coord3> chunksToUpdateSet;
        private readonly Queue<Coord3> chunksToRemove;
        private readonly HashSet<Coord3> chunksToRemoveSet;
        private readonly Material material;
        private readonly ArrayPool<Block> blockPool;
        private readonly ChunkMeshGenerator meshGenerator;

        private const int CHUNKS_UPDATE_PER_FRAME = 50;
        private const int CHUNKS_REMOVE_PER_FRAME = 50;

        public BlockWorld(Engine engine, Scene scene, string name, Material material) : base(scene, name)
        {
            this.engine = engine;
            this.material = material;

            chunks = new Dictionary<Coord3, LoadedChunk>();
            chunksToUpdate = new PriorityQueue<Coord3>();
            chunksToUpdateSet = new HashSet<Coord3>();
            chunksToRemove = new Queue<Coord3>();
            chunksToRemoveSet = new HashSet<Coord3>();
            blockPool = ArrayPool<Block>.Shared;
            meshGenerator = new ChunkMeshGenerator();
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

        public Chunk CreateChunk(Coord3 coord)
        {
            return new Chunk(coord, blockPool);
        }

        private void UpdateChunks()
        {
            var chunksUpdatedThisFrame = 0;
            while (chunksUpdatedThisFrame < CHUNKS_UPDATE_PER_FRAME && chunksToUpdate.Count > 0)
            {
                var chunkCoord = chunksToUpdate.Dequeue();
                chunksToUpdateSet.Remove(chunkCoord);

                if (chunksToRemoveSet.Contains(chunkCoord))
                    continue;

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
                chunksToRemoveSet.Remove(chunkCoord);

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
                chunk.Chunk.Dispose();
                chunks.Remove(chunkCoord);
            }
        }

        public void AddChunk(Chunk chunk)
        {
            if (chunks.ContainsKey(chunk.Coordinate))
                return;

            chunks.Add(chunk.Coordinate, new LoadedChunk(chunk));

            QueueSurroundingChunksForUpdate(chunk.Coordinate);

            // Can return early as we don't need to update anything as this chunk is fully inactive
            if (!chunk.IsAnyBlockActive)
                return;

            QueueCoordForUpdate(chunk.Coordinate);
        }

        public void RemoveChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate) || chunksToRemoveSet.Contains(chunk.Coordinate))
                return;

            chunksToRemove.Enqueue(chunk.Coordinate);
            chunksToRemoveSet.Add(chunk.Coordinate);

            QueueSurroundingChunksForUpdate(chunk.Coordinate);
        }

        public void UpdateChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate))
                return;

            QueueCoordForUpdate(chunk.Coordinate);

            QueueSurroundingChunksForUpdate(chunk.Coordinate);
        }

        public void RemoveAllChunks()
        {
            chunksToUpdate.Clear();
            chunksToUpdateSet.Clear();
            chunksToRemove.Clear();
            chunksToRemoveSet.Clear();

            foreach (var chunk in chunks.Values.ToList())
            {
                if (chunk.Renderable != null)
                {
                    RemoveComponent(chunk.Renderable);
                    chunk.Renderable.Dispose();
                }
                if (chunk.Physics != null)
                {
                    RemoveComponent(chunk.Physics);
                }
                chunk.Chunk.Dispose();
                chunks.Remove(chunk.Chunk.Coordinate);
            }
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
            return chunk.Blocks[(int)blockIndex.X + ((int)blockIndex.Y * Chunk.CHUNK_X_SIZE) + ((int)blockIndex.Z * Chunk.CHUNK_X_SIZE * Chunk.CHUNK_Y_SIZE)];
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
            if (chunksToUpdateSet.Contains(chunkCoord))
                return;

            var dist = PlayerInChunkCoord - chunkCoord;
            var distanceFromPlayer = (int)Math.Abs(Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y + dist.Z * dist.Z));
            chunksToUpdate.Enqueue(chunkCoord, distanceFromPlayer);
            chunksToUpdateSet.Add(chunkCoord);
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
                meshGenerator.GenerateMesh(chunk, this, out var vertices, out var vertexCount, out var indices, out var indexCount);
                if (loadedChunk.Renderable != null)
                {
                    loadedChunk.Renderable.UpdateChunk(vertices, vertexCount, indices, indexCount);
                    if (vertexCount <= 0)
                    {
                        RemoveComponent(loadedChunk.Renderable);
                        loadedChunk.Renderable.Dispose();

                        loadedChunk.Renderable = null;
                    }
                }
                else
                {
                    if (vertexCount > 0)
                    {
                        var renderable = new ChunkRenderable(chunk, meshGenerator, engine, material);
                        renderable.UpdateChunk(vertices, vertexCount, indices, indexCount);

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
                    if (loadedChunk.Physics is PhysicsBoxComponent)
                    {
                        RemoveComponent(loadedChunk.Physics);

                        loadedChunk.Physics = null;
                    }
                    else if (loadedChunk.Physics is PhysicsChunkComponent chunkComponent)
                    {
                        chunkComponent.UpdateChunk();
                    }
                }

                if (loadedChunk.Physics == null)
                {
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
                        var compound = new PhysicsChunkComponent(chunk, PhysicsInteractivity.Static)
                        {
                            PositionOffset = chunk.WorldPosition + (Chunk.CHUNK_SIZE * Chunk.CHUNK_BLOCK_RATIO * 0.5f)
                        };
                        physics = compound;
                    }

                    loadedChunk.Physics = physics;

                    AddComponent(physics);
                }
            }
        }

        private void QueueSurroundingChunksForUpdate(Coord3 coord)
        {
            void QueueChunkForUpdateIfExists(Coord3 coordOffset)
            {
                var chunkAbove = FindChunkByCoordinate(coord + coordOffset);
                if (chunkAbove == null)
                    return;

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
