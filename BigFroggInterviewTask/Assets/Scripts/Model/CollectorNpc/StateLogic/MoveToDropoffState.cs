using BigFroggInterviewTask.Model.CollectorNpc;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is moving to a dropoff location for the box it is carrying.
    /// </summary>
    public class MoveToDropoffState : MovingState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "MoveToDropoff"; } }

        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected override int TimerSetpoint { get { return context.TicksPerStepWithBox; } }

        /// <summary>
        /// The state to transition into if the NPC reaches the destination.
        /// </summary>
        protected override State NextStateIfDestinationReached { get { return new DropOffBoxState(context, path.Destination); } }

        /// <summary>
        /// The state to transition into if the path is interrupted.
        /// </summary>
        protected override State NextStateIfPathInterrupted { get { return new FindPathToDropoffState(context); }
 }
        /// <summary>
        /// Constructs a Pathfinding state from a set of unsorted box locations.
        /// </summary>
        public MoveToDropoffState(StateContext context, Path pathToDropff) : base(context, pathToDropff) { }

    }
}
