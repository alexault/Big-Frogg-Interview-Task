using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using BigFroggInterviewTask.Model.CollectorNpc;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Tests.Model
{
    /// <summary>
    /// Test the functionality of the CollectorNpcModel pathfinder helper class.
    /// </summary>
    public class CollectorNpcPathfinderTests
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
        /// A simple entity type used to provide obstacles for pathfinding.
        /// </summary>
        public class ObstacleEntity : EntityModel
        {
            public override string Name { get { return "ObstacleEntity"; } }
        }

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
            // Enable WorldModel and CollectorNpcModel logging.
            Log.ClearAllFlags();
            Log.EnableFlag(Log.Flag.WorldModelTrace);
            Log.EnableFlag(Log.Flag.CollectorNpcPathFinderTrace);

            // Create the world used for testing
            world = new WorldModel(Configuration.Load<WorldModel.Configuration>(ConfigurationPath + DefaultWorldConfiguration));
        }

        /// <summary>
        /// Verify that the shortest path between two points on the same row with no obstacles between them is found.
        /// </summary>
        [Test]
        public void VerifySimplePath()
        {
            // Expected path from O to D:
            //
            // |
            // |
            // |O...D
            // +----------

            Vector2Int origin = new Vector2Int(0, 0);

            List<Vector2Int> destinations = new List<Vector2Int>
            {
                new Vector2Int(4, 0),
            };

            List<Vector2Int?> expectedPath = new List<Vector2Int?>
            {
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0),
            };

            Path path = Path.FindShortestPath(world, origin, destinations);

            Assert.That(path, Is.Not.Null);
            Assert.That(path.Destination, Is.EqualTo(destinations[0]));

            VerifyExpectedPath(path, expectedPath);
        }

        /// <summary>
        /// Verify that the shortest path which avoids obstacles between two points on the same row is found.
        /// </summary>
        [Test]
        public void VerifyCollisionAvoidance()
        {
            // Expected path from O to D:

            // |
            // |...
            // |.x.
            // |.x.
            // |OxD
            // +----------

            Vector2Int origin = new Vector2Int(0, 0);

            List<Vector2Int> destinations = new List<Vector2Int>
            {
                new Vector2Int(2, 0),
            };

            List<Vector2Int> blockers = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
            };

            List<Vector2Int?> expectedPath = new List<Vector2Int?>
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3),
                new Vector2Int(2, 2),
                new Vector2Int(2, 1),
            };

            SpawnBlockers(blockers);
            Path path = Path.FindShortestPath(world, origin, destinations);

            Assert.That(path, Is.Not.Null);
            Assert.That(path.Destination, Is.EqualTo(destinations[0]));

            VerifyExpectedPath(path, expectedPath);
        }

        /// <summary>
        /// Verify that the path finds the closest destination in the list.
        /// </summary>
        [Test]
        public void VerifyClosestDestinationSelected()
        {
            // Expected path from O to D:
            // |
            // |....D
            // |.x 
            // |.x 
            // |OxD
            // +----------

            Vector2Int origin = new Vector2Int(0, 0);

            List<Vector2Int> destinations = new List<Vector2Int>
            {
                new Vector2Int(4, 3),
                new Vector2Int(2, 0),
            };

            List<Vector2Int> blockers = new List<Vector2Int>
            {
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
            };

            List<Vector2Int?> expectedPath = new List<Vector2Int?>
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3),
                new Vector2Int(3, 3),
            };

            SpawnBlockers(blockers);
            Path path = Path.FindShortestPath(world, origin, destinations);

            Assert.That(path, Is.Not.Null);
            Assert.That(path.Destination, Is.EqualTo(destinations[0]));

            VerifyExpectedPath(path, expectedPath);
        }

        /// <summary>
        /// Verify detection of obstacles spawned after path generation.
        /// </summary>
        [Test]
        public void VerifyCollisionDetection()
        {
            // Expected path from O to D:
            //
            // |
            // |
            // |O...D
            // +----------

            Vector2Int origin = new Vector2Int(0, 0);

            List<Vector2Int> destinations = new List<Vector2Int>
            {
                new Vector2Int(4, 0),
            };

            Path path = Path.FindShortestPath(world, origin, destinations);
            Assert.That(path.IsPathPassable(world), Is.True);

            // Spawn obstacle to block path:
            //
            // |
            // |
            // |O.x.D
            // +----------

            List<Vector2Int> blockers = new List<Vector2Int>
            {
                new Vector2Int(2, 0),
            };

            SpawnBlockers(blockers);
            Assert.That(path.IsPathPassable(world), Is.False);
        }

        /// <summary>
        /// Add a set of blocking entities to the world.
        /// </summary>
        private void SpawnBlockers(List<Vector2Int> locations)
        {
            foreach (Vector2Int location in locations)
            {
                world.AddEntity(new ObstacleEntity(), location);
            }
        }

        /// <summary>
        /// Verify that a generated path matches an expected path.
        /// </summary>
        private void VerifyExpectedPath(Path path, List<Vector2Int?> expectedSteps)
        {
            Assert.That(path.StepsRemaining, Is.EqualTo(expectedSteps.Count));
            Assert.That(path.IsPathPassable(world), Is.True);

            foreach (Vector2Int? expectedStep in expectedSteps)
            {
                Vector2Int actualStep = path.PopNextStep();
                if (expectedStep.HasValue)
                {
                    Assert.That(actualStep, Is.EqualTo(expectedStep));
                }
            }
        }
    }
}
