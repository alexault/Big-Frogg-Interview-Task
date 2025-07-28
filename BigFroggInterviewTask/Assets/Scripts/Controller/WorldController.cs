using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model;
using BigFroggInterviewTask.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Controller
{
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

        private float tickFrequency = 0.1f;
        private float nextTick = 0.0f;

        private WorldModel worldModel;

        [SerializeField]
        private WorldView worldView;

        [SerializeField]
        private BoxView boxView;

        [SerializeField]
        private CollectorNpcView collectorNpcView;

        void Start()
        {
            Log.ClearAllFlags();
            Log.EnableFlag(Log.Flag.CollectorNpcModelTrace);

            InitializeModels();
            InitializeViews();
        }

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

        private void InitializeViews()
        {
            Dictionary<Type, EntityView> entityViewMap = new Dictionary<Type, EntityView>()
            {
                { typeof(BoxModel), boxView },
                { typeof(CollectorNpcModel), collectorNpcView },
            };

            worldView.InitializeView(worldModel, entityViewMap);
        }

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