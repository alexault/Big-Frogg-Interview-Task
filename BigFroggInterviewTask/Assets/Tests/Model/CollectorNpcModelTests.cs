using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

namespace BigFroggInterviewTask.Tests.Model
{
    /// <summary>
    /// Test the functionality of the CollectorNpcModel class.
    /// </summary>
    public class CollectorNpcModelTests
    {
        /// <summary>
        /// The location of configuration files for this test set.
        /// </summary>
        private static string ConfigurationPath = $"{Application.dataPath}/Tests/Model/Configuration/CollectorNpcModel/";

        /// <summary>
        /// The default world configuration file.
        /// </summary>
        private const string DefaultWorldConfiguration = "DefaultWorld.json";

        /// <summary>
        /// The default world configuration file.
        /// </summary>
        private const string DefaultCollectorNpcConfiguration = "DefaultCollectorNpc.json";

        private struct CollectorNpcState
        {
            public Vector2Int Location;
            public bool? HasCollectedBox;
            public BoxModel.BoxColor? BoxColor;
            public bool? IsStuck;
        };

        /// <summary>
        /// The world model to be used for testing.
        /// </summary>
        private WorldModel world;

        /// <summary>
        /// The collector NPC model to be tested.
        /// </summary>
        private CollectorNpcModel collectorNpc;

        /// <summary>
        /// The configuration data used to generate the collector NPC model.
        /// </summary>
        private CollectorNpcModel.Configuration collectorNpcConfig;

        /// <summary>
        /// Setup common to all tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Enable WorldModel and CollectorNpcModel logging.
            Log.ClearAllFlags();
            Log.EnableFlag(Log.Flag.WorldModelTrace);
            Log.EnableFlag(Log.Flag.CollectorNpcModelTrace);
            Log.EnableFlag(Log.Flag.CollectorNpcPathFinderTrace);
            Log.EnableFlag(Log.Flag.CollectorNpcRuleHandlerTrace);

            // Create the world used for testing
            world = new WorldModel(Configuration.Load<WorldModel.Configuration>(ConfigurationPath + DefaultWorldConfiguration));
        }

        /// <summary>
        /// Loads the collector NPC model from a config file.
        /// </summary>
        private void LoadCollectorNpcFromConfig(string configName)
        {
            collectorNpcConfig = Configuration.Load<CollectorNpcModel.Configuration>(ConfigurationPath + configName);
            collectorNpc = new CollectorNpcModel(collectorNpcConfig);
        }

        /// <summary>
        /// Verify that a collector NPC entity can be added to the world.
        /// </summary>
        [Test]
        public void VerifyCollectorNpcCreation()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world.
            Vector2Int startLocation = new Vector2Int(5, 5);
            SpawnCollector(startLocation);

            // Verify collector exists in world
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation },
            };
            RunModelAndVerifyCollectorState(expectedStates);
        }

        /// <summary>
        /// Verify that the collector NPC does not move while there are no boxes in the world.
        /// </summary>
        [Test]
        public void VerifyIdleWhenWorldEmpty()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world.
            Vector2Int startLocation = new Vector2Int(5, 5);
            SpawnCollector(startLocation);

            // Collector should remain in the same position while there are no boxes to collect.
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
            };

            RunModelAndVerifyCollectorState(expectedStates);
        }

        /// <summary>
        /// Verify that the collector NPC does not move while all the boxes in the world are already sorted to the correct side.
        /// </summary>
        [Test]
        public void VerifyIdleWhenBoxesSorted()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world.
            Vector2Int startLocation = new Vector2Int(5, 5);
            SpawnCollector(startLocation);

            // Collector should remain in the same position while all boxes are sorted.
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false, IsStuck = false },    // Idle
            };

            // Add a red box to the left column and a blue box to the right column. Verify no collector movement.
            SpawnBox(new Vector2Int(0, 0), BoxModel.BoxColor.Red);
            SpawnBox(new Vector2Int(world.Size.x - 1, 0), BoxModel.BoxColor.Blue);
            RunModelAndVerifyCollectorState(expectedStates);

            // Fill the left column with red boxes and add a box to the second column. Verify no collector movement.
            for (int y = 1; y < world.Size.y; y++)
            {
                SpawnBox(new Vector2Int(0, y), BoxModel.BoxColor.Red);
            }
            SpawnBox(new Vector2Int(1, 5), BoxModel.BoxColor.Red);
            RunModelAndVerifyCollectorState(expectedStates);

            // Fill the right column with blue boxes and add a box to the secound column. Verify no collector movement.
            for (int y = 1; y < world.Size.y; y++)
            {
                SpawnBox(new Vector2Int(world.Size.x - 1, y), BoxModel.BoxColor.Blue);
            }
            SpawnBox(new Vector2Int(world.Size.x - 2, world.Size.y - 1), BoxModel.BoxColor.Blue);
            RunModelAndVerifyCollectorState(expectedStates);
        }

        /// <summary>
        /// Verify that the collector NPC detects an unsorted box and moves to the nearest space adjacent to it.
        /// </summary>
        [Test]
        public void VerifySimpleMoveToBox()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(0, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world
            SpawnBox(startLocation + (Vector2Int.right * 5), BoxModel.BoxColor.Red);

            // TODO: Implement FindPathToBox and MoveToBox states.
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, IsStuck = true }, // Idle -> FindPathToBox -> MoveToBox transition not yet implemented
            };

            RunModelAndVerifyCollectorState(expectedStates);
        }

        /// <summary>
        /// Add the collector NPC entity to the world at the given location.
        /// </summary>
        private void SpawnCollector(Vector2Int location)
        {
            world.AddEntity(collectorNpc, location);
        }

        /// <summary>
        /// Add a box to the world at the given location.
        /// </summary>
        private void SpawnBox(Vector2Int location, BoxModel.BoxColor color)
        {
            world.AddEntity(new BoxModel(new BoxModel.Configuration { Color = color }), location);
        }

        /// <summary>
        /// Verify that the collector NPC is in the expected state after each tick of the world model.
        /// </summary>
        private void RunModelAndVerifyCollectorState(List<CollectorNpcState> expectedStates)
        {
            foreach (CollectorNpcState expectedState in expectedStates)
            {
                world.Update();

                Dictionary<Vector2Int, CollectorNpcModel> collectors = world.GetAllEntities<CollectorNpcModel>();
                Assert.That(collectors.Count, Is.EqualTo(1));

                if (expectedState.Location != null)
                {
                    Assert.That(collectors.ContainsKey(expectedState.Location), Is.True);
                    Assert.That(collectors[expectedState.Location], Is.EqualTo(collectorNpc));
                    Assert.That(world.GetEntityAt(expectedState.Location), Is.EqualTo(collectorNpc));
                }

                if (expectedState.HasCollectedBox.HasValue)
                {
                    Assert.That(collectorNpc.HasCollectedBox, Is.EqualTo(expectedState.HasCollectedBox.Value));
                }

                if (expectedState.BoxColor.HasValue)
                {
                    Assert.That(collectorNpc.CollectedBoxColor, Is.EqualTo(expectedState.BoxColor.Value));
                }

                if (expectedState.IsStuck.HasValue)
                {
                    Assert.That(collectorNpc.IsStuck, Is.EqualTo(expectedState.IsStuck.Value));
                }
            }
        }
    }
}