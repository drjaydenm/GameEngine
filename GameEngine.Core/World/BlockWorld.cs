using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameEngine.Core.Entities;
using GameEngine.Core.Graphics;

namespace GameEngine.Core.World
{
    public class BlockWorld : Entity
    {
        public IEnumerable<Chunk> Chunks => chunks.Values;
        public int ChunksToUpdateCount => chunksToUpdate.Count;

        private readonly Engine engine;
        private readonly ConcurrentDictionary<Coord3, Chunk> chunks;
        private readonly ConcurrentQueue<Chunk> chunksToUpdate;
        private readonly ConcurrentDictionary<Chunk, ChunkRenderable> chunkRenderables;
        private readonly ConcurrentDictionary<Chunk, PhysicsComponent> chunkPhysics;
        private readonly Material material;

        private const int CHUNKS_UPDATE_PER_FRAME = 20;

        public BlockWorld(Engine engine, Scene scene, string name) : base(scene, name)
        {
            this.engine = engine;

            chunks = new ConcurrentDictionary<Coord3, Chunk>();
            chunksToUpdate = new ConcurrentQueue<Chunk>();
            chunkRenderables = new ConcurrentDictionary<Chunk, ChunkRenderable>();
            chunkPhysics = new ConcurrentDictionary<Chunk, PhysicsComponent>();
            material = new Material(engine, ShaderCode.VertexCode, ShaderCode.FragmentCode);
        }

        public override void Update()
        {
            base.Update();

            if (!chunksToUpdate.IsEmpty)
            {
                UpdateChunks();
            }
        }

        private void UpdateChunks()
        {
            var chunksUpdatedThisFrame = 0;
            while (chunksUpdatedThisFrame < CHUNKS_UPDATE_PER_FRAME && chunksToUpdate.TryDequeue(out var chunk))
            {
                AddOrUpdateRenderable(chunk);
                AddOrUpdatePhysics(chunk);

                chunksUpdatedThisFrame++;
            }
        }

        public void AddChunk(Chunk chunk)
        {
            if (chunks.ContainsKey(chunk.Coordinate))
                return;

            chunks.TryAdd(chunk.Coordinate, chunk);
            chunksToUpdate.Enqueue(chunk);

            //QueueSurroundingChunksForUpdate(chunk);
        }

        public void RemoveChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate))
                return;

            chunks.TryRemove(chunk.Coordinate, out var actualChunk);

            if (chunkRenderables.ContainsKey(actualChunk))
            {
                chunkRenderables.TryRemove(actualChunk, out var renderable);
                if (renderable != null)
                {
                    RemoveComponent(renderable);
                    renderable.Dispose();
                }
            }
            if (chunkPhysics.ContainsKey(actualChunk))
            {
                chunkPhysics.TryRemove(actualChunk, out var physics);
                if (physics != null)
                {
                    RemoveComponent(physics);
                }
            }

            // TODO update surrounding chunks
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

            return chunks[coordinate];
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

            var blockIndex = position - (Chunk.CHUNK_SIZE * chunk.Coordinate);
            return chunk.Blocks[(int)Math.Floor(blockIndex.X), (int)Math.Floor(blockIndex.Y), (int)Math.Floor(blockIndex.Z)];
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

        private void AddOrUpdateRenderable(Chunk chunk)
        {
            var chunkShouldRender = chunk.IsAnyBlockActive();

            // Make sure a key exists
            if (!chunkRenderables.ContainsKey(chunk))
                chunkRenderables.TryAdd(chunk, null);

            if (!chunkShouldRender && chunkRenderables[chunk] != null)
            {
                RemoveComponent(chunkRenderables[chunk]);
                chunkRenderables[chunk].Dispose();
                    
                chunkRenderables[chunk] = null;
            }
            else
            {
                var mesh = ChunkMeshGenerator.GenerateMesh(chunk, this);
                if (chunkRenderables[chunk] != null)
                {
                    chunkRenderables[chunk].UpdateChunk(mesh);
                    if (mesh == null)
                    {
                        RemoveComponent(chunkRenderables[chunk]);
                        chunkRenderables[chunk].Dispose();

                        chunkRenderables[chunk] = null;
                    }
                }
                else
                {
                    var renderable = new ChunkRenderable(chunk, engine, material);
                    renderable.UpdateChunk(mesh);

                    if (mesh != null)
                    {
                        chunkRenderables[chunk] = renderable;

                        AddComponent(renderable);
                    }
                    else
                    {
                        renderable.Dispose();
                    }
                }
            }
        }

        private void AddOrUpdatePhysics(Chunk chunk)
        {
            // Make sure a key exists
            if (!chunkPhysics.ContainsKey(chunk))
                chunkPhysics.TryAdd(chunk, null);

            if (chunkPhysics[chunk] == null && chunk.IsAnyBlockActive())
            {
                PhysicsComponent physics;
                var isChunkSolid = !chunk.IsAnyBlockInactive();

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
                                    compound.AddBox(new Vector3(x, y, z), Vector3.One);
                            }
                        }
                    }
                    physics = compound;
                }

                chunkPhysics[chunk] = physics;

                AddComponent(physics);
            }
        }

        private void QueueSurroundingChunksForUpdate(Chunk chunk)
        {
            void QueueChunkForUpdateIfExists(Coord3 coordOffset)
            {
                var chunkAbove = FindChunkByOffset(chunk, coordOffset);
                if (chunkAbove != null && !chunksToUpdate.Contains(chunkAbove))
                    chunksToUpdate.Enqueue(chunkAbove);
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
