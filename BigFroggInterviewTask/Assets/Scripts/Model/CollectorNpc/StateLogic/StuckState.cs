using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents any state where the NPC is stuck and cannot determine its next action.
    /// </summary>
    public class StuckState : State
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "Stuck"; } }

        /// <summary>
        /// Constructs a Stuck state.
        /// </summary>
        public StuckState(StateContext context) : base(context) { }

        /// <summary>
        /// Process the collector NPC logic for the Stuck state.
        /// </summary>
        public override ProcessResult ProcessTick(WorldModel world, Vector2Int npcLocation)
        {
            // We are stuck, there is nothing to do and no state to advance to.
            return ProcessResult.ProcessComplete;
        }
    }
}
