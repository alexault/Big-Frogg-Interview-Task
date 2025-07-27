using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model.CollectorNpc;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is idling until a box that needs sorted spawns.
    /// </summary>
    public class IdleState : TimerState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "Idle"; } }

        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected override int TimerSetpoint { get { return 1; } }

        /// <summary>
        /// Constructs an Idle state.
        /// </summary>
        public IdleState(StateContext context) : base(context) { }

        /// <summary>
        /// Check whether an unsorted box has appeared in the world, and advance to the FindPathToBoxState state if so.
        /// </summary>
        protected override ProcessResult TimerExpired(StateContext context, WorldModel world, Vector2Int npcLocation)
        {
            List<Vector2Int> unsortedBoxes = RuleHandler.FindAllUnsortedBoxes(context.Rules, world);
            Log.Write(Log.Flag.CollectorNpcModelTrace, $"Idle state found {unsortedBoxes.Count} unsorted boxes");

            if (unsortedBoxes.Count > 0)
            {
                // Transition immediately into FindPathToBoxState.
                context.State = new FindPathToBoxState(context, unsortedBoxes);
                return ProcessResult.ProcessNextState;
            }
            else
            {
                // Nothing more to do on this world tick.
                return ProcessResult.ProcessComplete;
            }
        }
    }
}