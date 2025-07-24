using BigFroggInterviewTask.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// An object spawner that spawns randomly colored boxes at random intervals.
    /// </summary>
    public class RandomBoxSpawnerModel : EntitySpawnerModel
    {
        /// <summary>
        /// The set of configuration data required to set up the box spawner.
        /// </summary>
        public struct Configuration
        {
            public int MinInterval;
            public int MaxInterval;
            public float BlueBoxRatio;
        };

        /// <summary>
        /// The minimum interval between box spawns.
        /// </summary>
        int minInterval;

        /// <summary>
        /// The maximum interval between box spawns.
        /// </summary>
        int maxInterval;

        /// <summary>
        /// The ratio of boxes which are blue.
        /// </summary>
        float blueBoxRatio;

        /// <summary>
        /// The number of ticks until the next box spawn.
        /// </summary>
        int ticksUntilNextSpawn;

        /// <summary>
        /// Creates a new box spawner.
        /// </summary>
        /// <param name="config">The configuration data used to create the spawner.</param>
        public RandomBoxSpawnerModel(Configuration config)
        {
            Log.Write(Log.Flag.RandomBoxSpawnerModelTrace, $"Creating random box spawner with interval ({config.MinInterval}-{config.MaxInterval}) and blue box ratio {config.BlueBoxRatio}");

            if (config.MinInterval < 1)
            {
                throw new ArgumentOutOfRangeException("config.MinInterval");
            }

            if (config.MaxInterval < config.MinInterval)
            {
                throw new ArgumentOutOfRangeException("config.MaxInterval");
            }

            if (config.BlueBoxRatio < 0.0f || config.BlueBoxRatio > 1.0f)
            {
                throw new ArgumentOutOfRangeException("config.BlueBoxRatio");
            }

            minInterval = config.MinInterval;
            maxInterval = config.MaxInterval;
            blueBoxRatio = config.BlueBoxRatio;

            ticksUntilNextSpawn = UnityEngine.Random.Range(minInterval, maxInterval);
        }

        public override void Update(WorldModel world)
        {
            ticksUntilNextSpawn--;
            Log.Write(Log.Flag.RandomBoxSpawnerModelTrace, $"Next box spawn in {ticksUntilNextSpawn} ticks");

            if (ticksUntilNextSpawn == 0)
            {
                List<Vector2Int> validSpawnLocations = world.GetAllEmptyTiles();

                if (validSpawnLocations.Count > 0 )
                {
                    // Select a random empty tile in the world to spawn the box
                    Vector2Int spawnLocation = validSpawnLocations[UnityEngine.Random.Range(0, validSpawnLocations.Count)];

                    // Select a random color for the box
                    BoxModel.BoxColor boxColor = UnityEngine.Random.Range(0.0f, 1.0f) < blueBoxRatio ? BoxModel.BoxColor.Blue : BoxModel.BoxColor.Red;

                    // Spawn the box and add it to the world
                    Log.Write(Log.Flag.RandomBoxSpawnerModelTrace, $"Spawning {boxColor} box at [{spawnLocation}]");
                    BoxModel box = new BoxModel(new BoxModel.Configuration { Color = boxColor });
                    world.AddEntity(box, spawnLocation);
                }
                else
                {
                    Log.Write(Log.Flag.RandomBoxSpawnerModelTrace, $"No valid locations to spawn box");
                }

                // Select a random interval for the next spawn
                ticksUntilNextSpawn = UnityEngine.Random.Range(minInterval, maxInterval + 1);
                Log.Write(Log.Flag.RandomBoxSpawnerModelTrace, $"Next box spawn in {ticksUntilNextSpawn} ticks");
            }
        }
    }
}