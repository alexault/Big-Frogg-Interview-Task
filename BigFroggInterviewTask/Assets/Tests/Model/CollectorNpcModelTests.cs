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

        private void SpawnCollector(Vector2Int location)
        {
            world.AddEntity(collectorNpc, location);
        }

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