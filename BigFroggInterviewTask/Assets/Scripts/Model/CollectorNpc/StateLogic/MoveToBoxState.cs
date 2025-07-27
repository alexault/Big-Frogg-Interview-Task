using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model.CollectorNpc;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is moving to an unsorted box.
    /// </summary>
    public class MoveToBoxState : MovingState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "MoveToBoxState"; } }

        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected override int TimerSetpoint { get { return context.TicksPerStepWithoutBox; } }

        /// <summary>
        /// The state to transition into if the NPC reaches the destination.
        /// </summary>
        protected override State GetNextStateIfDestinationReached() { return new StuckState(context); }  // TODO: Implement CollectBoxState

        /// <summary>
        /// The state to transition into if the path is interrupted.
        /// </summary>
        protected override State GetNextStateIfPathInterrupted(WorldModel world)
        {
            List<Vector2Int> unsortedBoxes = RuleHandler.FindAllUnsortedBoxes(context.Rules, world);

            if (unsortedBoxes.Count > 0)
            {
                return new FindPathToBoxState(context, unsortedBoxes);
            }
            else
            {
                return new IdleState(context);
            }
        }

        /// <summary>
        /// Constructs a Pathfinding state from a set of unsorted box locations.
        /// </summary>
        public MoveToBoxState(StateContext context, Path pathToBox) : base(context, pathToBox) { }

    }
}
