using System;
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
        public int ActualRenderableCount => chunkRenderables.Where(r => r.Value != null).Count();
        public int ActualPhysicsCount => chunkPhysics.Where(r => r.Value != null).Count();

        private readonly Engine engine;
        private readonly Dictionary<Coord3, Chunk> chunks;
        private readonly Queue<Coord3> chunksToUpdate;
        private readonly Queue<Coord3> chunksToRemove;
        private readonly Dictionary<Coord3, ChunkRenderable> chunkRenderables;
        private readonly Dictionary<Coord3, PhysicsComponent> chunkPhysics;
        private readonly Material material;

        private const int CHUNKS_UPDATE_PER_FRAME = 10;
        private const int CHUNKS_REMOVE_PER_FRAME = 10;

        public BlockWorld(Engine engine, Scene scene, string name) : base(scene, name)
        {
            this.engine = engine;

            chunks = new Dictionary<Coord3, Chunk>();
            chunksToUpdate = new Queue<Coord3>();
            chunksToRemove = new Queue<Coord3>();
            chunkRenderables = new Dictionary<Coord3, ChunkRenderable>();
            chunkPhysics = new Dictionary<Coord3, PhysicsComponent>();
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
                var chunk = chunksToRemove.Dequeue();
                if (chunkRenderables.ContainsKey(chunk))
                {
                    var renderable = chunkRenderables[chunk];
                    chunkRenderables.Remove(chunk);
                    if (renderable != null)
                    {
                        RemoveComponent(renderable);
                        renderable.Dispose();
                    }
                }
                if (chunkPhysics.ContainsKey(chunk))
                {
                    var physics = chunkPhysics[chunk];
                    chunkPhysics.Remove(chunk);
                    if (physics != null)
                    {
                        RemoveComponent(physics);
                    }
                }
            }
        }

        public void AddChunk(Chunk chunk)
        {
            if (chunks.ContainsKey(chunk.Coordinate))
                return;

            chunks.Add(chunk.Coordinate, chunk);
            chunksToUpdate.Enqueue(chunk.Coordinate);

            QueueSurroundingChunksForUpdate(chunk);
        }

        public void RemoveChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate))
                return;

            chunks.Remove(chunk.Coordinate);
            chunksToRemove.Enqueue(chunk.Coordinate);

            // TODO update surrounding chunks
        }

        public void UpdateChunk(Chunk chunk)
        {
            if (!chunks.ContainsKey(chunk.Coordinate))
                return;

            chunksToUpdate.Enqueue(chunk.Coordinate);

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

        private void AddOrUpdateRenderable(Coord3 chunkCoord)
        {
            var chunk = chunks[chunkCoord];
            var chunkShouldRender = chunk.IsAnyBlockActive;

            // Make sure a key exists
            if (!chunkRenderables.ContainsKey(chunkCoord))
                chunkRenderables.Add(chunkCoord, null);

            if (!chunkShouldRender && chunkRenderables[chunkCoord] != null)
            {
                RemoveComponent(chunkRenderables[chunkCoord]);
                chunkRenderables[chunkCoord].Dispose();
                    
                chunkRenderables[chunkCoord] = null;
            }
            else
            {
                var mesh = ChunkMeshGenerator.GenerateMesh(chunk, this);
                if (chunkRenderables[chunkCoord] != null)
                {
                    chunkRenderables[chunkCoord].UpdateChunk(mesh);
                    if (mesh == null)
                    {
                        RemoveComponent(chunkRenderables[chunkCoord]);
                        chunkRenderables[chunkCoord].Dispose();

                        chunkRenderables[chunkCoord] = null;
                    }
                }
                else
                {
                    var renderable = new ChunkRenderable(chunk, engine, material);
                    renderable.UpdateChunk(mesh);

                    if (mesh != null)
                    {
                        chunkRenderables[chunkCoord] = renderable;

                        AddComponent(renderable);
                    }
                    else
                    {
                        renderable.Dispose();
                    }
                }
            }
        }

        private void AddOrUpdatePhysics(Coord3 chunkCoord)
        {
            // Make sure a key exists
            if (!chunkPhysics.ContainsKey(chunkCoord))
                chunkPhysics.Add(chunkCoord, null);

            var chunk = chunks[chunkCoord];
            var chunkShouldHavePhysics = chunk.IsAnyBlockActive;

            if (chunkPhysics[chunkCoord] != null && !chunkShouldHavePhysics)
            {
                RemoveComponent(chunkPhysics[chunkCoord]);

                chunkPhysics[chunkCoord] = null;
            }
            else if (chunkShouldHavePhysics)
            {
                // If we already have a physics object, remove it
                if (chunkPhysics[chunkCoord] != null)
                {
                    RemoveComponent(chunkPhysics[chunkCoord]);

                    chunkPhysics[chunkCoord] = null;
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
                                    compound.AddBox(new Vector3(x, y, z), Vector3.One);
                            }
                        }
                    }
                    physics = compound;
                }

                chunkPhysics[chunkCoord] = physics;

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

                if (chunkAbove != null && !chunksToUpdate.Contains(chunkAbove.Coordinate))
                    chunksToUpdate.Enqueue(chunkAbove.Coordinate);
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
