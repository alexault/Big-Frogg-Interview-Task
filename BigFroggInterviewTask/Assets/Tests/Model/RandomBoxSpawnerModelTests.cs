using BigFroggInterviewTask.Config;
using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Tests.Model
{
    /// <summary>
    /// Test the functionality of the RandomBoxSpawnerModel class.
    /// </summary>
    public class RandomBoxSpawnerModelTests
    {
        /// <summary>
        /// The location of configuration files for this test set.
        /// </summary>
        private static string ConfigurationPath = $"{Application.dataPath}/Tests/Model/Configuration/RandomBoxSpawnerModel/";

        /// <summary>
        /// The default world configuration file.
        /// </summary>
        private const string DefaultWorldConfiguration = "DefaultWorld.json";

        /// <summary>
        /// The default spawner configuration file.
        /// </summary>
        private const string DefaultSpawnerConfiguration = "DefaultSpawner.json";

        /// <summary>
        /// A spawner configuration file that spawns blue boxes at every tick.
        /// </summary>
        private const string BlueBoxSpawnerConfiguration = "BlueBoxSpawner.json";

        /// <summary>
        /// A spawner configuration file that spawns red boxes at every tick.
        /// </summary>
        private const string RedBoxSpawnerConfiguration = "RedBoxSpawner.json";

        /// <summary>
        /// The world model to be used for testing.
        /// </summary>
        private WorldModel world;

        /// <summary>
        /// Setup common to all tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Enable only WorldModel logging.
            Log.ClearAllFlags();
            Log.EnableFlag(Log.Flag.WorldModelTrace);
            Log.EnableFlag(Log.Flag.RandomBoxSpawnerModelTrace);

            // Create the world used for testing
            world = new WorldModel(Configuration.Load<WorldModel.Configuration>(ConfigurationPath + DefaultWorldConfiguration));
        }

        /// <summary>
        /// Verify that boxes spawn within the expected interval.
        /// </summary>
        [Test]
        public void VerifyRandomInterval()
        {
            // Create the box spawner and add it to the world
            RandomBoxSpawnerModel.Configuration config = Configuration.Load<RandomBoxSpawnerModel.Configuration>(ConfigurationPath + DefaultSpawnerConfiguration);
            RandomBoxSpawnerModel spawner = new RandomBoxSpawnerModel(config);
            world.Spawners.Add(spawner);

            // Verify that initial world contains no boxes
            Assert.That(world.GetAllEntities<BoxModel>().Count, Is.EqualTo(0));

            // Spawn ten boxes and verify that the interval between each spawn is within the expected range
            for (int boxNumber = 0; boxNumber < 10; boxNumber++)
            {
                // Run the world update loop until either a box is spawned or the maximum interval to spawn a box is past
                int ticks = 0;
                while (world.GetAllEntities<BoxModel>().Count <= boxNumber && ticks < config.MaxInterval)
                {
                    world.Update();
                    ticks++;
                }

                // Verify that exactly one box was spawned during this interval
                Assert.That(world.GetAllEntities<BoxModel>().Count, Is.EqualTo(boxNumber + 1));

                // Verify that the box was spanwed within the desired interval
                Assert.That(ticks, Is.AtLeast(config.MinInterval));
                Assert.That(ticks, Is.AtMost(config.MaxInterval));
            }
        }

        /// <summary>
        /// Verify that boxes do not spawn when there are no valid spawn locations.
        /// </summary>
        [Test]
        public void VerifyNoMoreSpawnsWhenWorldIsFull()
        {
            // Create the box spawner and add it to the world
            RandomBoxSpawnerModel spawner = new RandomBoxSpawnerModel(Configuration.Load<RandomBoxSpawnerModel.Configuration>(ConfigurationPath + BlueBoxSpawnerConfiguration));
            world.Spawners.Add(spawner);

            // Fill the world with boxes
            for (int x = 0; x < world.Size.x; x++)
            {
                for (int y = 0; y < world.Size.y; y++)
                {
                    world.AddEntity(new BoxModel(new BoxModel.Configuration { Color = BoxModel.BoxColor.Blue }), new Vector2Int(x, y));
                }
            }

            // Verify that every tile contains a box
            Assert.That(world.GetAllEntities<BoxModel>().Count, Is.EqualTo(world.Size.x * world.Size.y));
            Assert.That(world.GetAllEmptyTiles().Count, Is.EqualTo(0));

            // Attempt to spawn another box
            world.Update();

            // Verify that no new box was spawned
            Assert.That(world.GetAllEntities<BoxModel>().Count, Is.EqualTo(world.Size.x * world.Size.y));
        }

        /// <summary>
        /// Verify that box colors spawn with the expected ratio.
        /// </summary>
        [Test]
        public void VerifyBoxColorDistribution()
        {
            RunBoxColorDistributionTest(BlueBoxSpawnerConfiguration, BoxModel.BoxColor.Blue);
            RunBoxColorDistributionTest(RedBoxSpawnerConfiguration, BoxModel.BoxColor.Red);
        }

        /// <summary>
        /// Verify that the given box spawner configuration spawns only the expected color box.
        /// </summary>
        private void RunBoxColorDistributionTest(string configName, BoxModel.BoxColor color)
        {
            // Create the box spawner and add it to the world
            RandomBoxSpawnerModel spawner = new RandomBoxSpawnerModel(Configuration.Load<RandomBoxSpawnerModel.Configuration>(ConfigurationPath + configName));
            world.Spawners.Add(spawner);

            // Verify that initial world contains no boxes
            Assert.That(world.GetAllEntities<BoxModel>().Count, Is.EqualTo(0));

            // Spawn ten boxes
            for (int i = 0; i < 10; i++)
            {
                world.Update();
            }
            Dictionary<Vector2Int, BoxModel> boxes = world.GetAllEntities<BoxModel>();
            Assert.That(boxes.Count, Is.EqualTo(10));

            // Verify that all spawned boxes are the expected color
            foreach (BoxModel box in boxes.Values)
            {
                Assert.That(box.Color, Is.EqualTo(color));
            }

            // Reset the world
            world.Spawners.Remove(spawner);
            foreach (KeyValuePair<Vector2Int, BoxModel> box in boxes)
            {
                world.RemoveEntity(box.Value, box.Key);
            }

            // Verify that final world contains no boxes
            Assert.That(world.GetAllEntities<BoxModel>().Count, Is.EqualTo(0));
        }
    }
}