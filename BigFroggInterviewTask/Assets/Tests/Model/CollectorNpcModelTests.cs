using BigFroggInterviewTask.Config;
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
            public Vector2Int? Location;
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

            // Collector should move toward the box after entering the MoveToBox state
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 1) },    // Idle -> MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 2) },    // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 3) },    // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 4) },    // MoveToBox
            };

            RunModelAndVerifyCollectorState(expectedStates);
        }

        /// <summary>
        /// Verify that the collector NPC collects an adjacent unsorted box.
        /// </summary>
        [Test]
        public void VerifyCollectBox()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(0, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world adjacent to the collector
            Vector2Int boxStartLocation = startLocation + (Vector2Int.right * 1);
            SpawnBox(boxStartLocation, BoxModel.BoxColor.Red);

            // Collector should collect the box
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // Idle -> CollectBox
            };

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify box was removed from world
            Assert.That(world.GetEntityAt(boxStartLocation), Is.Null);
        }

        /// <summary>
        /// Verify that the collector NPC moves to an unsorted box and collects it.
        /// </summary>
        [Test]
        public void VerifyMoveToAndCollectBox()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(0, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world
            Vector2Int boxStartLocation = startLocation + (Vector2Int.right * 5);
            SpawnBox(boxStartLocation, BoxModel.BoxColor.Red);

            // Collector should move toward the box after entering the MoveToBox state, then collect it after entering the CollectBox state
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 1), HasCollectedBox = false },                                   // Idle -> MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 2), HasCollectedBox = false },                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 3), HasCollectedBox = false },                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 4), HasCollectedBox = false },                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 4), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },  // MoveToBox -> CollectBox
            };

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify box was removed from world
            Assert.That(world.GetEntityAt(boxStartLocation), Is.Null);
        }

        /// <summary>
        /// Verify that the collector NPC collects an unsorted box and moves to a potential dropoff location.
        /// </summary>
        [Test]
        public void VerifyCollectBoxAndMoveToDropoff()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(4, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world adjacent to the collector
            Vector2Int boxStartLocation = startLocation + (Vector2Int.right * 1);
            SpawnBox(boxStartLocation, BoxModel.BoxColor.Red);

            // Collector collects the box after entering the CollectBox state, then move to the left after entering the MoveToDropoff state
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },                           // Idle -> CollectBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 1), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // CollectBox -> MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 2), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 3), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
            };

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify box was removed from world
            Assert.That(world.GetEntityAt(boxStartLocation), Is.Null);
        }

        /// <summary>
        /// Verify that the collector NPC collects an unsorted box and drops it into a sorted location.
        /// </summary>
        [Test]
        public void VerifyCollectAndDropoffBox()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(1, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world adjacent to the collector
            Vector2Int boxStartLocation = startLocation + (Vector2Int.right * 1);
            SpawnBox(boxStartLocation, BoxModel.BoxColor.Red);

            // Collector collect the box after entering the CollectBox state, then move to the left after entering the MoveToDropoff state
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // Idle -> CollectBox
                new CollectorNpcState { Location = startLocation, HasCollectedBox = false},                                     // CollectBox -> DropOffBox
            };
            Vector2Int expectedBoxDropLocation = startLocation + (Vector2Int.left * 1);

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify box was removed from original location and placed in new location
            Assert.That(world.GetEntityAt(boxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(expectedBoxDropLocation), Is.TypeOf<BoxModel>());
        }

        /// <summary>
        /// Verify that the collector NPC collects an unsorted box, moves, and drops it into a sorted location.
        /// </summary>
        [Test]
        public void VerifyCollectMoveAndDropoffBox()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(4, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world adjacent to the collector
            Vector2Int boxStartLocation = startLocation + (Vector2Int.right * 1);
            SpawnBox(boxStartLocation, BoxModel.BoxColor.Red);

            // Collector collects the box after entering the CollectBox state, then move to the left after entering the MoveToDropoff state
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },                           // Idle -> CollectBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 1), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // CollectBox -> MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 2), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 3), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 3), HasCollectedBox = false},                                     // MoveToDropoff -> DropOffBox
            };
            Vector2Int expectedBoxDropLocation = startLocation + (Vector2Int.left * 4);

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify box was removed from original location and placed in new location
            Assert.That(world.GetEntityAt(boxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(expectedBoxDropLocation), Is.TypeOf<BoxModel>());
        }

        /// <summary>
        /// Verify that the collector NPC moves to and collects an unsorted box, then moves to and drops it into a sorted location.
        /// </summary>
        [Test]
        public void VerifyFullCollectorBehavior()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(0, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world
            Vector2Int boxStartLocation = startLocation + (Vector2Int.right * 5);
            SpawnBox(boxStartLocation, BoxModel.BoxColor.Red);

            // Collector moves to and collects the box, then moves to and drops it in a sorted location
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 1), HasCollectedBox = false },                                   // Idle -> MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 2), HasCollectedBox = false },                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 3), HasCollectedBox = false },                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 4), HasCollectedBox = false },                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 4), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },  // MoveToBox -> CollectBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 3), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // CollectBox -> MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 2), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 1), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 1), HasCollectedBox = false},                                     // MoveToDropoff -> DropOffBox
            };
            Vector2Int expectedBoxDropLocation = startLocation;

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify box was removed from original location and placed in new location
            Assert.That(world.GetEntityAt(boxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(expectedBoxDropLocation), Is.TypeOf<BoxModel>());
        }

        /// <summary>
        /// Verify that the collector NPC collects and sorts all boxes.
        /// </summary>
        [Test]
        public void VerifyFullCollectorBehaviorMultipleBoxes()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(0, 0);
            SpawnCollector(startLocation);

            // Add one box of each color to the collector
            Vector2Int redBoxStartLocation = new Vector2Int(4, 0);
            Vector2Int blueBoxStartLocation = new Vector2Int(5, 0);
            SpawnBox(redBoxStartLocation, BoxModel.BoxColor.Red);
            SpawnBox(blueBoxStartLocation, BoxModel.BoxColor.Blue);

            // Collector collects and sorts the red box to the left, then the blue box to the right
            List<CollectorNpcState> expectedStates = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = new Vector2Int(1, 0), HasCollectedBox = false },                                     // Idle -> MoveToBox
                new CollectorNpcState { Location = new Vector2Int(2, 0), HasCollectedBox = false },                                     // MoveToBox
                new CollectorNpcState { Location = new Vector2Int(3, 0), HasCollectedBox = false },                                     // MoveToBox
                new CollectorNpcState { Location = new Vector2Int(3, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },    // MoveToBox -> CollectBox
                new CollectorNpcState { Location = new Vector2Int(2, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },    // CollectBox -> MoveToDropoff
                new CollectorNpcState { Location = new Vector2Int(1, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },    // MoveToDropoff
                new CollectorNpcState { Location = new Vector2Int(1, 0), HasCollectedBox = false },                                     // MoveToDropoff -> DropOffBox
                new CollectorNpcState { Location = new Vector2Int(2, 0), HasCollectedBox = false },                                     // DropOffBox -> MoveToBox
                new CollectorNpcState { Location = new Vector2Int(3, 0), HasCollectedBox = false },                                     // MoveToBox
                new CollectorNpcState { Location = new Vector2Int(4, 0), HasCollectedBox = false },                                     // MoveToBox
                new CollectorNpcState { Location = new Vector2Int(4, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },   // MoveToBox -> CollectBox
                new CollectorNpcState { Location = new Vector2Int(5, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },   // CollectBox -> MoveToDropoff
                new CollectorNpcState { Location = new Vector2Int(6, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },   // MoveToDropoff
                new CollectorNpcState { Location = new Vector2Int(7, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },   // MoveToDropoff
                new CollectorNpcState { Location = new Vector2Int(8, 0), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },   // MoveToDropoff
                new CollectorNpcState { Location = new Vector2Int(8, 0), HasCollectedBox = false },                                     // MoveToDropoff -> DropOffBox
            };
            Vector2Int expectedRedBoxDropLocation = new Vector2Int(0, 0);
            Vector2Int expectedBlueBoxDropLocation = new Vector2Int(9, 0);

            RunModelAndVerifyCollectorState(expectedStates);

            // Verify boxes were removed from original location and placed in new location
            Assert.That(world.GetEntityAt(redBoxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(blueBoxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(expectedRedBoxDropLocation), Is.TypeOf<BoxModel>());
            Assert.That((world.GetEntityAt(expectedRedBoxDropLocation) as BoxModel).Color, Is.EqualTo(BoxModel.BoxColor.Red));
            Assert.That(world.GetEntityAt(expectedBlueBoxDropLocation), Is.TypeOf<BoxModel>());
            Assert.That((world.GetEntityAt(expectedBlueBoxDropLocation) as BoxModel).Color, Is.EqualTo(BoxModel.BoxColor.Blue));
        }

        /// <summary>
        /// Verify that the collector NPC re-routes to the nearest unsorted box when its original path is interrupted
        /// </summary>
        [Test]
        public void VerifyMoveToBoxPathInterrupted()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(0, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world
            Vector2Int redBoxStartLocation = startLocation + (Vector2Int.right * 5);
            SpawnBox(redBoxStartLocation, BoxModel.BoxColor.Red);

            // Collector begins moving toward the box
            List<CollectorNpcState> expectedStatesBeforeBoxSpawn = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 1), HasCollectedBox = false },   // Idle -> MoveToBox
            };
            RunModelAndVerifyCollectorState(expectedStatesBeforeBoxSpawn);

            // Add a new box in the path of the collector
            Vector2Int blueBoxStartLocation = startLocation + (Vector2Int.right * 4);
            SpawnBox(blueBoxStartLocation, BoxModel.BoxColor.Blue);

            // Collector collects and sorts the new box instead
            List<CollectorNpcState> expectedStatesAfterBoxSpawn = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 2), HasCollectedBox = false },                                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 3), HasCollectedBox = false },                                                   // MoveToBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 3), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },                 // MoveToBox -> CollectBox
                new CollectorNpcState { HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue },                                                                    // CollectBox -> MoveToDropoff (can either move right or up)
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 4) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue }, // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 5) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue }, // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 6) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue }, // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 7) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue }, // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 8) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Blue }, // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.right * 8) + Vector2Int.up, HasCollectedBox = false},                                    // MoveToDropoff -> DropOffBox
            };
            Vector2Int expectedBlueBoxDropLocation = startLocation + (Vector2Int.right * 9) + Vector2Int.up;

            RunModelAndVerifyCollectorState(expectedStatesAfterBoxSpawn);

            // Verify blue box was removed from original location and placed in new location
            Assert.That(world.GetEntityAt(blueBoxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(expectedBlueBoxDropLocation), Is.TypeOf<BoxModel>());
            Assert.That((world.GetEntityAt(expectedBlueBoxDropLocation) as BoxModel).Color, Is.EqualTo(BoxModel.BoxColor.Blue));

            // Verify red box remains in original location
            Assert.That(world.GetEntityAt(redBoxStartLocation), Is.TypeOf<BoxModel>());
            Assert.That((world.GetEntityAt(redBoxStartLocation) as BoxModel).Color, Is.EqualTo(BoxModel.BoxColor.Red));
        }

        /// <summary>
        /// Verify that the collector NPC re-routes to the nearest dropoff location when its original path is interrupted
        /// </summary>
        [Test]
        public void VerifyMoveToDropoffPathInterrupted()
        {
            LoadCollectorNpcFromConfig(DefaultCollectorNpcConfiguration);

            // Add a collector to the world
            Vector2Int startLocation = new Vector2Int(4, 0);
            SpawnCollector(startLocation);

            // Add a single box to the world adjacent to the collector
            Vector2Int redBoxStartLocation = startLocation + (Vector2Int.right * 1);
            SpawnBox(redBoxStartLocation, BoxModel.BoxColor.Red);

            // Collector moves to and collects the box, then moves to and drops it in a sorted location
            List<CollectorNpcState> expectedStatesBeforeBoxSpawn = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },                           // Idle -> CollectBox
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 1), HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // CollectBox -> MoveToDropoff

            };
            RunModelAndVerifyCollectorState(expectedStatesBeforeBoxSpawn);

            // Add a new box in the path of the collector
            Vector2Int blueBoxStartLocation = startLocation + (Vector2Int.left * 2);
            SpawnBox(blueBoxStartLocation, BoxModel.BoxColor.Blue);

            // Collector routes around the new box
            List<CollectorNpcState> expectedStatesAfterBoxSpawn = new List<CollectorNpcState>
            {
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 1) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 2) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 3) + Vector2Int.up, HasCollectedBox = true, BoxColor = BoxModel.BoxColor.Red },   // MoveToDropoff
                new CollectorNpcState { Location = startLocation + (Vector2Int.left * 3) + Vector2Int.up, HasCollectedBox = false},                                     // MoveToDropoff -> DropOffBox
            };
            Vector2Int expectedRedBoxDropLocation = startLocation + (Vector2Int.left * 4) + Vector2Int.up;

            RunModelAndVerifyCollectorState(expectedStatesAfterBoxSpawn);

            // Verify red box was removed from original location and placed in new location
            Assert.That(world.GetEntityAt(redBoxStartLocation), Is.Null);
            Assert.That(world.GetEntityAt(expectedRedBoxDropLocation), Is.TypeOf<BoxModel>());
            Assert.That((world.GetEntityAt(expectedRedBoxDropLocation) as BoxModel).Color, Is.EqualTo(BoxModel.BoxColor.Red));

            // Verify blue box remains in original location
            Assert.That(world.GetEntityAt(blueBoxStartLocation), Is.TypeOf<BoxModel>());
            Assert.That((world.GetEntityAt(blueBoxStartLocation) as BoxModel).Color, Is.EqualTo(BoxModel.BoxColor.Blue));
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

                if (expectedState.Location.HasValue)
                {
                    Assert.That(collectors.ContainsKey(expectedState.Location.Value), Is.True);
                    Assert.That(collectors[expectedState.Location.Value], Is.EqualTo(collectorNpc));
                    Assert.That(world.GetEntityAt(expectedState.Location.Value), Is.EqualTo(collectorNpc));
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