using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using BigFroggInterviewTask.Model.CollectorNpc;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Tests.Model
{
    /// <summary>
    /// Test the functionality of the CollectorNpcModel rule handler helper class.
    /// </summary>
    public class CollectorNpcRuleHandlerTests
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
        /// The default ruleset, with red boxes sorted to the left and blue boxes sorted to the right.
        /// </summary>
        public RuleHandler.RuleSet DefaultRules = new RuleHandler.RuleSet
        {
            SortingRules = new Dictionary<BoxModel.BoxColor, RuleHandler.WorldSide>
            {
                { BoxModel.BoxColor.Red, RuleHandler.WorldSide.Left },
                { BoxModel.BoxColor.Blue, RuleHandler.WorldSide.Right },
            }
        };

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
            Log.EnableFlag(Log.Flag.CollectorNpcRuleHandlerTrace);

            // Create the world used for testing
            world = new WorldModel(Configuration.Load<WorldModel.Configuration>(ConfigurationPath + DefaultWorldConfiguration));
        }

        /// <summary>
        /// Verify detection of sorted vs unsorted boxes.
        /// </summary>
        [Test]
        public void VerifySimpleBoxSorting()
        {
            // Add some boxes already sorted into the correct side of the world
            Dictionary<Vector2Int, BoxModel.BoxColor> sortedBoxes = new Dictionary<Vector2Int, BoxModel.BoxColor>
            {
                { new Vector2Int(0, 0), BoxModel.BoxColor.Red },
                { new Vector2Int(0, 2), BoxModel.BoxColor.Red },
                { new Vector2Int(0, world.Size.y - 1), BoxModel.BoxColor.Red },

                { new Vector2Int(world.Size.x - 1, 0), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - 1, 2), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - 1, world.Size.y - 1), BoxModel.BoxColor.Blue },
            };
            SpawnBoxes(sortedBoxes);

            // Add some boxes not sorted into the correct side of the world
            Dictionary<Vector2Int, BoxModel.BoxColor> unsortedBoxes = new Dictionary<Vector2Int, BoxModel.BoxColor>
            {
                { new Vector2Int(1, 0), BoxModel.BoxColor.Red },
                { new Vector2Int(2, 2), BoxModel.BoxColor.Red },
                { new Vector2Int(3, world.Size.y - 1), BoxModel.BoxColor.Red },

                { new Vector2Int(world.Size.x - 4, 0), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - 3, 2), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - 2, world.Size.y - 1), BoxModel.BoxColor.Blue },
            };
            SpawnBoxes(unsortedBoxes);

            // Generate the list of unsorted boxes
            List<Vector2Int> unsortedBoxResult = RuleHandler.FindAllUnsortedBoxes(DefaultRules, world);

            // Verify that unsorted box list contains all the unsorted boxes
            foreach (Vector2Int unsortedBoxLocation in unsortedBoxes.Keys)
            {
                Assert.That(unsortedBoxResult.Contains(unsortedBoxLocation), Is.True);
            }

            // Verify that unsorted box list contains none of the sorted boxes
            foreach (Vector2Int sortedBoxLocation in sortedBoxes.Keys)
            {
                Assert.That(unsortedBoxResult.Contains(sortedBoxLocation), Is.False);
            }
        }

        /// <summary>
        /// Verify detection of sorted vs unsorted boxes when one or more levels are already full of boxes.
        /// </summary>
        [Test]
        public void VerifyMultiLevelBoxSorting()
        {
            // Fill the left and two rightmost columns with sorted boxes
            int fullLeftRows = 1;
            int fullRightRows = 2;
            for (int y = 0; y < world.Size.y; y++)
            {
                for (int x = 0; x < fullLeftRows; x++)
                {
                    world.AddEntity(new BoxModel(new BoxModel.Configuration { Color = BoxModel.BoxColor.Red }), new Vector2Int(x, y));
                }

                for (int x = world.Size.x - 1; x > world.Size.x - fullRightRows - 1; x--)
                {
                    world.AddEntity(new BoxModel(new BoxModel.Configuration { Color = BoxModel.BoxColor.Blue }), new Vector2Int(x, y));
                }
            }

            // Add some boxes sorted into the first empty column for each color
            Dictionary<Vector2Int, BoxModel.BoxColor> sortedBoxes = new Dictionary<Vector2Int, BoxModel.BoxColor>
            {
                { new Vector2Int(fullLeftRows, 0), BoxModel.BoxColor.Red },
                { new Vector2Int(fullLeftRows, 2), BoxModel.BoxColor.Red },
                { new Vector2Int(fullLeftRows, world.Size.y - 1), BoxModel.BoxColor.Red },

                { new Vector2Int(world.Size.x - fullRightRows - 1, 0), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - fullRightRows - 1, 2), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - fullRightRows - 1, world.Size.y - 1), BoxModel.BoxColor.Blue },
            };
            SpawnBoxes(sortedBoxes);

            // Add some boxes not sorted into the correct side of the world
            Dictionary<Vector2Int, BoxModel.BoxColor> unsortedBoxes = new Dictionary<Vector2Int, BoxModel.BoxColor>
            {
                { new Vector2Int(fullLeftRows + 1, 0), BoxModel.BoxColor.Red },
                { new Vector2Int(fullLeftRows + 2, 2), BoxModel.BoxColor.Red },
                { new Vector2Int(fullLeftRows + 3, world.Size.y - 1), BoxModel.BoxColor.Red },

                { new Vector2Int(world.Size.x - fullRightRows - 4, 0), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - fullRightRows - 3, 2), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - fullRightRows - 2, world.Size.y - 1), BoxModel.BoxColor.Blue },
            };
            SpawnBoxes(unsortedBoxes);

            // Generate the list of unsorted boxes
            List<Vector2Int> unsortedBoxResult = RuleHandler.FindAllUnsortedBoxes(DefaultRules, world);

            // Verify that unsorted box list contains all the unsorted boxes
            foreach (Vector2Int unsortedBoxLocation in unsortedBoxes.Keys)
            {
                Assert.That(unsortedBoxResult.Contains(unsortedBoxLocation), Is.True);
            }

            // Verify that unsorted box list contains none of the sorted boxes
            foreach (Vector2Int sortedBoxLocation in sortedBoxes.Keys)
            {
                Assert.That(unsortedBoxResult.Contains(sortedBoxLocation), Is.False);
            }
        }

        /// <summary>
        /// Verify detection of sorted vs unsorted boxes.
        /// </summary>
        [Test]
        public void VerifySimpleDropoffLocationDetection()
        {
            // Add some boxes already occupying the right column
            Dictionary<Vector2Int, BoxModel.BoxColor> sortedBoxes = new Dictionary<Vector2Int, BoxModel.BoxColor>
            {
                { new Vector2Int(0, 0), BoxModel.BoxColor.Red },
                { new Vector2Int(0, 2), BoxModel.BoxColor.Red },
                { new Vector2Int(0, world.Size.y - 1), BoxModel.BoxColor.Red },

                { new Vector2Int(world.Size.x - 1, 0), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - 1, world.Size.y - 3), BoxModel.BoxColor.Blue },
                { new Vector2Int(world.Size.x - 1, world.Size.y - 1), BoxModel.BoxColor.Blue },
            };
            SpawnBoxes(sortedBoxes);


            // Generate the list of dropoff locations for red boxes
            List<Vector2Int> redDropoffLocations = RuleHandler.GetDropoffLocationsForBox(DefaultRules, world, BoxModel.BoxColor.Red);

            foreach (Vector2Int dropoffLocation in redDropoffLocations)
            {
                // Verify that all dropoff locations are in the leftmost column
                Assert.That(dropoffLocation.x, Is.EqualTo(0));

                // Verify that all dropoff locations are empty
                Assert.That(world.GetEntityAt(dropoffLocation), Is.Null);
            }

            // Verify that all valid dropoff locations are in the list
            for (int y = 0; y < world.Size.y; y++)
            {
                Vector2Int location = new Vector2Int(0, y);
                if (world.GetEntityAt(location) == null)
                {
                    Assert.That(redDropoffLocations.Contains(location), Is.True);
                }
                else
                {
                    Assert.That(redDropoffLocations.Contains(location), Is.False);
                }
            }


            // Generate the list of dropoff locations for blue boxes
            List<Vector2Int> blueDropoffLocations = RuleHandler.GetDropoffLocationsForBox(DefaultRules, world, BoxModel.BoxColor.Blue);

            foreach (Vector2Int dropoffLocation in blueDropoffLocations)
            {
                // Verify that all dropoff locations are in the rightmost column
                Assert.That(dropoffLocation.x, Is.EqualTo(world.Size.x - 1));

                // Verify that all dropoff locations are empty
                Assert.That(world.GetEntityAt(dropoffLocation), Is.Null);
            }

            // Verify that all valid dropoff locations are in the list
            for (int y = 0; y < world.Size.y; y++)
            {
                Vector2Int location = new Vector2Int(world.Size.x - 1, y);
                if (world.GetEntityAt(location) == null)
                {
                    Assert.That(blueDropoffLocations.Contains(location), Is.True);
                }
                else
                {
                    Assert.That(blueDropoffLocations.Contains(location), Is.False);
                }
            }
        }

        /// <summary>
        /// Verify retreival of box dropoff locations when one or more levels are already full of boxes.
        /// </summary>
        [Test]
        public void VerifyMultiLeveDropoffLocationDetection()
        {
            // Fill the two leftmost columns with sorted boxes
            int fullLeftRows = 2;
            for (int y = 0; y < world.Size.y; y++)
            {
                for (int x = 0; x < fullLeftRows; x++)
                {
                    world.AddEntity(new BoxModel(new BoxModel.Configuration { Color = BoxModel.BoxColor.Red }), new Vector2Int(x, y));
                }
            }

            // Add some boxes occupying first empty column
            Dictionary<Vector2Int, BoxModel.BoxColor> sortedBoxes = new Dictionary<Vector2Int, BoxModel.BoxColor>
            {
                { new Vector2Int(fullLeftRows, 0), BoxModel.BoxColor.Red },
                { new Vector2Int(fullLeftRows, 2), BoxModel.BoxColor.Red },
                { new Vector2Int(fullLeftRows, world.Size.y - 1), BoxModel.BoxColor.Red },
            };
            SpawnBoxes(sortedBoxes);

            // Generate the list of dropoff locations
            List<Vector2Int> dropoffLocations = RuleHandler.GetDropoffLocationsForBox(DefaultRules, world, BoxModel.BoxColor.Red);

            foreach (Vector2Int dropoffLocation in dropoffLocations)
            {
                // Verify that all dropoff locations are in the leftmost unfilled column
                Assert.That(dropoffLocation.x, Is.EqualTo(fullLeftRows));

                // Verify that all dropoff locations are empty
                Assert.That(world.GetEntityAt(dropoffLocation), Is.Null);
            }

            // Verify that all valid dropoff locations are in the list
            for (int y = 0; y < world.Size.y; y++)
            {
                Vector2Int location = new Vector2Int(fullLeftRows, y);
                if (world.GetEntityAt(location) == null)
                {
                    Assert.That(dropoffLocations.Contains(location), Is.True);
                }
                else
                {
                    Assert.That(dropoffLocations.Contains(location), Is.False);
                }
            }
        }

        /// <summary>
        /// Add a set of boxes to the world.
        /// </summary>
        private void SpawnBoxes(Dictionary<Vector2Int, BoxModel.BoxColor> boxes)
        {
            foreach (KeyValuePair<Vector2Int, BoxModel.BoxColor> box in boxes)
            {
                Vector2Int location = box.Key;
                BoxModel.BoxColor color = box.Value;
                world.AddEntity(new BoxModel(new BoxModel.Configuration { Color = color }), location);
            }
        }
    }
}
