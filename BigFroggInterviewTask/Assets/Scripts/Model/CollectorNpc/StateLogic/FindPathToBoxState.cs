using BigFroggInterviewTask.Model.CollectorNpc;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is finding the shortest path to any unsorted box.
    /// </summary>
    public class FindPathToBoxState : PathfindingState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "FindPathToBox"; } }

        /// <summary>
        /// The set of unsorted boxes.
        /// </summary>
        protected override List<Vector2Int> GetPotentialDestinations(WorldModel world) { return RuleHandler.FindAllUnsortedBoxes(context.Rules, world); }

        /// <summary>
        /// The state to transition into if no unsorted boxes are found.
        /// </summary>
        protected override State NextStateIfNoDestinationsFound { get { return new IdleState(context); }
 }
        /// <summary>
        /// The state to transition into if the NPC is already adjacent to an unsorted box.
        /// </summary>
        protected override State NextStateIfZeroLengthPathFound { get { return new CollectBoxState(context, path.Destination); }
 }
        /// <summary>
        /// The state to transition into if a path to an unsorted box is found.
        /// </summary>
        protected override State NextStateIfNonzeroLengthPathFound { get { return new MoveToBoxState(context, path); }
 }
        /// <summary>
        /// The state to transition into if no path to any of the unsorted boxes is found.
        /// </summary>
        protected override State NextStateIfNoPathFound { get { return new StuckState(context); }
 }
        /// <summary>
        /// Constructs a Pathfinding state from a set of unsorted box locations.
        /// </summary>
        public FindPathToBoxState(StateContext context) : base(context) { }

    }
}
