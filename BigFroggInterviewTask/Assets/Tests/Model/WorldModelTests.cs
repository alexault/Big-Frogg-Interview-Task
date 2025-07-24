using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Tests.Model
{
    /// <summary>
    /// Test the functionality of the WorldModel class.
    /// </summary>
    public class WorldModelTests
    {
        /// <summary>
        /// The location of configuration files for this test set.
        /// </summary>
        private static string ConfigurationPath = $"{Application.dataPath}/Tests/Model/Configuration/WorldModel/";

        /// <summary>
        /// The default world configuration file.
        /// </summary>
        private static string DefaultWorldConfiguration = "DefaultWorld.json";

        /// <summary>
        /// A simple entity type used for testing.
        /// </summary>
        public class TestEntity : EntityModel
        {
            public override string Name { get { return "TestEntity"; } }
        }

        /// <summary>
        /// A simple entity type used for testing.
        /// </summary>
        public class TestEntityDifferentType : EntityModel
        {
            public override string Name { get { return "TestEntityDifferentType"; } }
        }

        /// <summary>
        /// The world model to be tested.
        /// </summary>
        private WorldModel world;

        /// <summary>
        /// The configuration data used to generate the world model.
        /// </summary>
        private WorldModel.Configuration worldConfig;

        /// <summary>
        /// Setup common to all tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Enable only WorldModel logging.
            Log.ClearAllFlags();
            Log.EnableFlag(Log.Flag.WorldModelTrace);
        }

        /// <summary>
        /// Loads the world model from a config file.
        /// </summary>
        private void LoadWorldFromConfig(string configName)
        {
            worldConfig = Configuration.Load<WorldModel.Configuration>(ConfigurationPath + configName);
            world = new WorldModel(worldConfig);
        }

        /// <summary>
        /// Verify that the world size matches the loaded configuration. Verify that locations within the world are accessable
        /// and that locations outside the bounds of the world throw an exception when trying to access them.
        /// </summary>
        [Test]
        public void VerifyWorldSize()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            // Verify the world size matches the loaded configuration
            Assert.That(world.Size.x, Is.EqualTo(worldConfig.Size.x));
            Assert.That(world.Size.y, Is.EqualTo(worldConfig.Size.y));

            // Verify that all four corners of the world are accessable
            Assert.That(() => world.GetEntityAt(new Vector2Int(0, 0)), Is.Null);
            Assert.That(() => world.GetEntityAt(new Vector2Int(worldConfig.Size.x - 1, 0)), Is.Null);
            Assert.That(() => world.GetEntityAt(new Vector2Int(0, worldConfig.Size.y - 1)), Is.Null);
            Assert.That(() => world.GetEntityAt(new Vector2Int(worldConfig.Size.x - 1, worldConfig.Size.y - 1)), Is.Null);

            // Verify that locations outside the world are not accessable
            Assert.That(() => world.GetEntityAt(new Vector2Int(-1, 0)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => world.GetEntityAt(new Vector2Int(0, -1)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => world.GetEntityAt(new Vector2Int(worldConfig.Size.x, worldConfig.Size.y - 1)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => world.GetEntityAt(new Vector2Int(worldConfig.Size.x - 1, worldConfig.Size.y)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that a generic entity can be added to a world model.
        /// </summary>
        [Test]
        public void VerifyThatEntityCanBeAdded()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            TestEntity entity = new TestEntity();
            Vector2Int location = new Vector2Int(1, 2);

            // Verify that entity is added
            world.AddEntity(entity, location);
            Assert.That(world.GetEntityAt(location), Is.EqualTo(entity));
        }

        /// <summary>
        /// Verify that the same entity cannot be added twice to a world model.
        /// </summary>
        [Test]
        public void VerifyThatDuplicateEntitiesCannotBeAdded()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            TestEntity entity = new TestEntity();
            Vector2Int location1 = new Vector2Int(0, 0);
            Vector2Int location2 = new Vector2Int(1, 1);

            // Verify that entity cannot be added to two different locations
            world.AddEntity(entity, location1);
            Assert.That(() => world.AddEntity(entity, location2), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that two different entities cannot be added to the same location of a world model.
        /// </summary>
        [Test]
        public void VerifyThatEntitiesCannotCollide()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            TestEntity entity1 = new TestEntity();
            TestEntity entity2 = new TestEntity();
            Vector2Int location = new Vector2Int(0, 0);

            // Verify that two different entities cannot be added to the same location
            world.AddEntity(entity1, location);
            Assert.That(() => world.AddEntity(entity2, location), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that a generic entity can be removed from a world model.
        /// </summary>
        [Test]
        public void VerifyThatEntityCanBeRemoved()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            TestEntity entity = new TestEntity();
            Vector2Int location = new Vector2Int(1, 2);

            // Verify that added entity can be removed
            world.AddEntity(entity, location);
            world.RemoveEntity(entity, location);
            Assert.That(world.GetEntityAt(location), Is.Null);
        }

        /// <summary>
        /// Verify that an entity cannot be removed from a location at which it does not exist.
        /// </summary>
        [Test]
        public void VerifyThatEntityCannotBeRemovedFromIncorrectLocation()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            TestEntity entity = new TestEntity();
            Vector2Int location1 = new Vector2Int(0, 0);
            Vector2Int location2 = new Vector2Int(1, 1);

            // Verify that entity cannot be removed from location at which it does not exist
            world.AddEntity(entity, location1);
            Assert.That(() => world.RemoveEntity(entity, location2), Throws.TypeOf<InvalidOperationException>());

        }

        /// <summary>
        /// Verify that a generic entity can be moved within a world model.
        /// </summary>
        [Test]
        public void VerifyThatEntityCanBeMoved()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            TestEntity entity = new TestEntity();
            Vector2Int location1 = new Vector2Int(0, 0);
            Vector2Int location2 = new Vector2Int(2, 3);

            // Verify that added entity can be moved
            world.AddEntity(entity, location1);
            world.MoveEntity(entity, location1, location2);
            Assert.That(world.GetEntityAt(location1), Is.Null);
            Assert.That(world.GetEntityAt(location2), Is.EqualTo(entity));
        }

        /// <summary>
        /// Verify that a list of all entities of a specific type can be retreived.
        /// </summary>
        [Test]
        public void VerifyThatEntityListCanBeRetreived()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            // Add entities of two different types to the world
            TestEntity entity1 = new TestEntity();
            Vector2Int location1 = new Vector2Int(0, 0);
            TestEntity entity2 = new TestEntity();
            Vector2Int location2 = new Vector2Int(1, 1);
            TestEntityDifferentType entity3 = new TestEntityDifferentType();
            Vector2Int location3 = new Vector2Int(2, 2);
            world.AddEntity(entity1, location1);
            world.AddEntity(entity2, location2);
            world.AddEntity(entity3, location3);

            // Get the list of all entities of TestEntity type
            Dictionary<Vector2Int, TestEntity> allTestEntities = world.GetAllEntities<TestEntity>();

            // Verify that the list includes both TestEntities
            Assert.That(allTestEntities.Count, Is.EqualTo(2));
            Assert.That(allTestEntities.ContainsKey(location1), Is.True);
            Assert.That(allTestEntities.ContainsKey(location2), Is.True);
            Assert.That(allTestEntities.ContainsValue(entity1), Is.True);
            Assert.That(allTestEntities.ContainsValue(entity2), Is.True);
            Assert.That(allTestEntities[location1], Is.EqualTo(entity1));
            Assert.That(allTestEntities[location2], Is.EqualTo(entity2));

            // Verify that the list does not include different types of entities 
            Assert.That(allTestEntities.ContainsKey(location3), Is.False);
        }

        /// <summary>
        /// Verify that a list of all empty tiles can be retreived.
        /// </summary>
        [Test]
        public void VerifyThatEmptyTileListCanBeRetreived()
        {
            LoadWorldFromConfig(DefaultWorldConfiguration);

            // Add some entities to the world
            TestEntity entity1 = new TestEntity();
            Vector2Int location1 = new Vector2Int(0, 0);
            TestEntity entity2 = new TestEntity();
            Vector2Int location2 = new Vector2Int(1, 1);
            TestEntityDifferentType entity3 = new TestEntityDifferentType();
            Vector2Int location3 = new Vector2Int(2, 2);
            world.AddEntity(entity1, location1);
            world.AddEntity(entity2, location2);
            world.AddEntity(entity3, location3);

            // Get the list of all empty tiles
            List<Vector2Int> allEmptyTiles = world.GetAllEmptyTiles();

            // Verify that the list includes all empty tiles
            Assert.That(allEmptyTiles.Count, Is.EqualTo(9));
            Assert.That(allEmptyTiles.Contains(new Vector2Int(0, 1)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(0, 2)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(0, 3)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(1, 0)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(1, 2)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(1, 3)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(2, 0)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(2, 1)), Is.True);
            Assert.That(allEmptyTiles.Contains(new Vector2Int(2, 3)), Is.True);

            // Verify that the list does not include any occupied tiles
            Assert.That(allEmptyTiles.Contains(location1), Is.False);
            Assert.That(allEmptyTiles.Contains(location2), Is.False);
            Assert.That(allEmptyTiles.Contains(location3), Is.False);
        }

    }
}
