using BigFroggInterviewTask.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is collecting an unsorted box.
    /// </summary>
    public class DropOffBoxState : TimerState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "DropOffBox"; } }

        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected override int TimerSetpoint { get { return context.TicksToDropBox; } }

        /// <summary>
        /// The location of the dropoff.
        /// </summary>
        private readonly Vector2Int DropoffLocation;

        /// <summary>
        /// Constructs a CollectBox state from the location of a box.
        /// </summary>
        public DropOffBoxState(StateContext context, Vector2Int dropoffLocation) : base(context)
        {
            DropoffLocation = dropoffLocation;
        }

        /// <summary>
        /// Check that a box exists at the target location and collect it if so.
        /// </summary>
        protected override ProcessResult TimerExpired(StateContext context, WorldModel world, Vector2Int npcLocation)
        {
            if (world.GetEntityAt(DropoffLocation) == null)
            {
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC dropping {context.Box.Color} box at [{DropoffLocation}]");

                // Add box to world and remove from state context
                world.AddEntity(context.Box, DropoffLocation);
                context.Box = null;

                context.State = new IdleState(context);
                return ProcessResult.ProcessComplete;
            }
            else
            {
                // Potential AI improvement: collector could check for dropoff location being filled during MoveToBoxState also
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC dropoff locaiton [{DropoffLocation}] no longer empty, finding new dropoff");
                context.State = new FindPathToDropoffState(context);
                return ProcessResult.ProcessComplete;
            }
        }
    }
}