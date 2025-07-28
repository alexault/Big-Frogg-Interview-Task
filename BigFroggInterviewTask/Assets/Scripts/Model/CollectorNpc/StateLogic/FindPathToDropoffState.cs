using BigFroggInterviewTask.Model.CollectorNpc;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is finding the shortest path to any dropoff location for the box it is carrying.
    /// </summary>
    public class FindPathToDropoffState : PathfindingState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "FindPathToDropoff"; } }

        /// <summary>
        /// The set of valid dropoff locations for the carried box.
        /// </summary>
        protected override List<Vector2Int> GetPotentialDestinations(WorldModel world) { return RuleHandler.GetDropoffLocationsForBox(context.Rules, world, context.Box.Color); }

        /// <summary>
        /// The state to transition into if no dropoff locations are found.
        /// </summary>
        // Potential AI improvement: collector could drop this box anywhere and try to find another box instead
        protected override State NextStateIfNoDestinationsFound { get { return new StuckState(context); }
 }
        /// <summary>
        /// The state to transition into if the NPC is already adjacent to a dropoff location.
        /// </summary>
        protected override State NextStateIfZeroLengthPathFound { get { return new DropOffBoxState(context, path.Destination); }
 }
        /// <summary>
        /// The state to transition into if a path to a dropoff location is found.
        /// </summary>
        protected override State NextStateIfNonzeroLengthPathFound { get { return new MoveToDropoffState(context, path); }
 }
        /// <summary>
        /// The state to transition into if no path to any dropoff location is found.
        /// </summary>
        protected override State NextStateIfNoPathFound { get { return new StuckState(context); }
 }
        /// <summary>
        /// Constructs a Pathfinding state from a set of unsorted box locations.
        /// </summary>
        public FindPathToDropoffState(StateContext context) : base(context) { }

    }
}
