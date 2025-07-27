using BigFroggInterviewTask.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// The model for the game world.
    /// </summary>
    public class WorldModel
    {
        /// <summary>
        /// The set of configuration data required to generate the world.
        /// </summary>
        public struct Configuration
        {
            public Vector2Int Size;
        };

        /// <summary>
        /// The size of the world, in tiles.
        /// </summary>
        public Vector2Int Size { get; }

        /// <summary>
        /// The list of entity spawners in the world.
        /// </summary>
        public List<EntitySpawnerModel> Spawners { get; }

        /// <summary>
        /// The tile matrix representing the game world and the entities occupying each tile.
        /// </summary>
        private EntityModel[,] tiles;

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="config">The configuration data used to create the world.</param>
        public WorldModel(Configuration config)
        {
            Log.Write(Log.Flag.WorldModelTrace, $"Creating world of size [{config.Size}]");

            if (!IsValidWorldSize(config.Size))
            {
                throw new ArgumentOutOfRangeException("config.Size");
            }

            Size = config.Size;
            Spawners = new List<EntitySpawnerModel>();
            tiles = new EntityModel[Size.x, Size.y];
        }

        /// <summary>
        /// Run the world logic and update its state.
        /// </summary>
        public void Update()
        {
            foreach (EntitySpawnerModel spawner in Spawners)
            {
                spawner.Update(this);
            }
        }

        /// <summary>
        /// Gets the entity at a single location.
        /// </summary>
        /// <param name="config">The location at which to get the entity.</param>
        /// <returns>The entity at the location, or null if no entity is present.</returns>
        public EntityModel GetEntityAt(Vector2Int location)
        {
            if (!IsLocationInRange(location))
            {
                throw new ArgumentOutOfRangeException("location");
            }

            return tiles[location.x, location.y];
        }

        /// <summary>
        /// Checks whether an entity exists anywhere in the world.
        /// </summary>
        /// <param name="entity">The entity to search for.</param>
        /// <returns>True if the entity exists in the world, false otherwise.</returns>
        public bool ContainsEntity(EntityModel entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    if (tiles[x, y] == entity)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a list of all entities of a given type in the world and their locations.
        /// </summary>
        /// <returns>A dictionary mapping all entities of the given type to their location in the world.</returns>
        public Dictionary<Vector2Int, T> GetAllEntities<T>() where T : EntityModel
        {
            Dictionary<Vector2Int, T> entityList = new Dictionary<Vector2Int, T>();

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    if (tiles[x, y] is T)
                    {
                        entityList[new Vector2Int(x, y)] = tiles[x, y] as T;
                    }
                }
            }

            return entityList;
        }

        /// <summary>
        /// Gets a list of all tiles in the world not occupied by any entity.
        /// </summary>
        /// <returns>A list of the locations of all unoccupied tiles in the world.</returns>
        public List<Vector2Int> GetAllEmptyTiles()
        {
            List<Vector2Int> emptyTiles = new List<Vector2Int>();

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    if (tiles[x, y] == null)
                    {
                        emptyTiles.Add(new Vector2Int(x, y));
                    }
                }
            }

            return emptyTiles;
        }

        /// <summary>
        /// Adds an entity to the world.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="location">The location in the world to place the entity.</param>
        public void AddEntity(EntityModel entity, Vector2Int location)
        {
            Log.Write(Log.Flag.WorldModelTrace, $"Adding entity [{entity}] to location [{location}]");

            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (!IsLocationInRange(location))
            {
                throw new ArgumentOutOfRangeException("location");
            }
            
            if (ContainsEntity(entity))
            {
                throw new InvalidOperationException($"AddEntity: World already contains entity [{entity}]");
            }

            if (tiles[location.x, location.y] != null)
            {
                throw new InvalidOperationException($"AddEntity: Location [{location}] already contains existing entity [{tiles[location.x, location.y]}]");
            }

            tiles[location.x, location.y] = entity;
        }

        /// <summary>
        /// Removes an entity from the world.
        /// </summary>
        /// <param name="entity">The entity to remote.</param>
        /// <param name="location">The location in the world from which to remove the entity.</param>
        public void RemoveEntity(EntityModel entity, Vector2Int location)
        {
            Log.Write(Log.Flag.WorldModelTrace, $"Removing entity [{entity}] from location [{location}]");

            if (!IsLocationInRange(location))
            {
                throw new ArgumentOutOfRangeException("location");
            }

            if (tiles[location.x, location.y] != entity)
            {
                throw new InvalidOperationException($"RemoveEntity: Location [{location}] does not contain entity [{entity}]");
            }

            tiles[location.x, location.y] = null;
        }

        /// <summary>
        /// Moves an entity to a different location in the world.
        /// </summary>
        /// <param name="entity">The entity to move.</param>
        /// <param name="originalLocation">The location in the world from which to move the entity.</param>
        /// <param name="newLocation">The location in the world to move the entity to.</param>
        public void MoveEntity(EntityModel entity, Vector2Int originalLocation, Vector2Int newLocation)
        {
            Log.Write(Log.Flag.WorldModelTrace, $"Moving entity [{entity}] from location [{originalLocation}] to location [{newLocation}]");

            RemoveEntity(entity, originalLocation);
            AddEntity(entity, newLocation);
        }

        /// <summary>
        /// Checks whether the given size is a valid size for a world.
        /// </summary>
        private bool IsValidWorldSize(Vector2Int size)
        {
            return
                size.x > 0 &&
                size.y > 0;
        }

        /// <summary>
        /// Checks whether the given size is within the size of the world.
        /// </summary>
        public bool IsLocationInRange(Vector2Int location)
        {
            return
                location.x >= 0 &&
                location.x < Size.x &&
                location.y >= 0 &&
                location.y < Size.y;
        }
    }
}
