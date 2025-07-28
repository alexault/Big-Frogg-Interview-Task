using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using BigFroggInterviewTask.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Controller
{
    /// <summary>
    /// The controller for the game world.
    /// </summary>
    public class WorldController : MonoBehaviour
    {
        /// <summary>
        /// The location of world configuration files.
        /// </summary>
        private static string ConfigurationPath = $"{Application.streamingAssetsPath}/Configuration/";

        /// <summary>
        /// The controller configuration file.
        /// </summary>
        [SerializeField]
        private string ControllerConfiguration;

        /// <summary>
        /// The set of configuration data required to generate the controller.
        /// </summary>
        public struct Configuration
        {
            public string WorldConfig;
            public string SpawnerConfig;
            public string CollectorNpcConfig;
            public Vector2Int CollectorNpcStartLocation;
            public float TickFrequency;
        };

        /// <summary>
        /// The frequency, in seconds, of game world frame ticks.
        /// </summary>
        private float tickFrequency;

        /// <summary>
        /// The time, in seconds, until the next game world frame tick.
        /// </summary>
        private float nextTick = 0.0f;

        /// <summary>
        /// The world model backing the controller.
        /// </summary>
        private WorldModel worldModel;

        /// <summary>
        /// The world view displaying model data.
        /// </summary>
        [SerializeField]
        private WorldView worldView;

        /// <summary>
        /// The box view displaying model data.
        /// </summary>
        [SerializeField]
        private BoxView boxView;

        /// <summary>
        /// The collector NPC view displaying model data.
        /// </summary>
        [SerializeField]
        private CollectorNpcView collectorNpcView;

        /// <summary>
        /// Initialize all models and views required to run the world controller.
        /// </summary>
        void Start()
        {
            Log.ClearAllFlags();

            InitializeModels();
            InitializeViews();
        }

        /// <summary>
        /// Initialize the controller models.
        /// </summary>
        private void InitializeModels()
        {
            Configuration config = Config.Configuration.Load<Configuration>(ConfigurationPath + ControllerConfiguration);

            tickFrequency = config.TickFrequency;

            WorldModel.Configuration worldConfig = Config.Configuration.Load<WorldModel.Configuration>(ConfigurationPath + config.WorldConfig);
            RandomBoxSpawnerModel.Configuration spawnerConfig = Config.Configuration.Load<RandomBoxSpawnerModel.Configuration>(ConfigurationPath + config.SpawnerConfig);
            CollectorNpcModel.Configuration collectorNpcConfig = Config.Configuration.Load<CollectorNpcModel.Configuration>(ConfigurationPath + config.CollectorNpcConfig);

            worldModel = new WorldModel(worldConfig);
            worldModel.Spawners.Add(new RandomBoxSpawnerModel(spawnerConfig));
            worldModel.AddEntity(new CollectorNpcModel(collectorNpcConfig), config.CollectorNpcStartLocation);
        }

        /// <summary>
        /// Initialize the controller views.
        /// </summary>
        private void InitializeViews()
        {
            Dictionary<Type, EntityView> entityViewMap = new Dictionary<Type, EntityView>()
            {
                { typeof(BoxModel), boxView },
                { typeof(CollectorNpcModel), collectorNpcView },
            };

            worldView.InitializeView(worldModel, entityViewMap);
        }

        /// <summary>
        /// Process world frame ticks.
        /// </summary>
        void Update()
        {
            nextTick -= Time.deltaTime;

            while (nextTick < 0.0f)
            {
                nextTick += tickFrequency;
                worldModel.Update();
                worldView.UpdateView(worldModel);
            }
        }
    }
}