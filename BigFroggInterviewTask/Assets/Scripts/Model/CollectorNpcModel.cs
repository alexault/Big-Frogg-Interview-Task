using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model.CollectorNpc;
using BigFroggInterviewTask.Model.StateLogic;
using UnityEngine;

namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// The model for an NPC that collects boxes and sorts them according to color.
    /// </summary>
    public partial class CollectorNpcModel : EntityModel
    {
        /// <summary>
        /// The set of data defining the configurable behavior of the NPC.
        /// </summary>
        public struct Configuration
        {
            public RuleHandler.WorldSide RedBoxSortingSide;
            public RuleHandler.WorldSide BlueBoxSortingSide;

            public int TicksPerStepWithoutBox;
            public int TicksPerStepWithBox;
            public int TicksToCollectBox;
            public int TicksToDropBox;
        };

        /// <summary>
        /// The entity name.
        /// </summary>
        public override string Name { get { return $"Collector NPC"; } }

        /// <summary>
        /// Indicates whether the NPC has collected a box.
        /// </summary>
        public bool HasCollectedBox { get { return stateContext.Box != null; } }

        /// <summary>
        /// Indicates the color of the box the NPC has collected, or None if the NPC has not collected a box.
        /// </summary>
        public BoxModel.BoxColor CollectedBoxColor { get { return stateContext.Box == null ? BoxModel.BoxColor.None : stateContext.Box.Color; } }

        /// <summary>
        /// Indicates whether the NPC has reached a point where it is unable to determine what action to take next.
        /// </summary>
        public bool IsStuck { get { return stateContext.IsStuck; } }

        /// <summary>
        /// The state machine governing the NPC's behavior.
        /// </summary>
        private StateContext stateContext;

        /// <summary>
        /// Creates a new collector NPC.
        /// </summary>
        /// <param name="config">The configuration data used to create the NPC.</param>
        public CollectorNpcModel(Configuration config)
        {
            Log.Write(Log.Flag.CollectorNpcModelTrace, $"Creating collector NPC model");

            stateContext = new StateContext(config);
            stateContext.State = new IdleState(stateContext);
        }

        /// <summary>
        /// Run the entity's logic and update its state.
        /// </summary>
        public override void Update(WorldModel world, Vector2Int location)
        {
            stateContext.ProcessTick(world, location);
        }
    }
}
