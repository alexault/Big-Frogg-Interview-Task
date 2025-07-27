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
        public override string Name { get { return "FindPathToBoxState"; } }

        /// <summary>
        /// The state to transition into if the NPC is already adjacent to an unsorted box.
        /// </summary>
        protected override State GetNextStateIfZeroLengthPathFound() { return new CollectBoxState(context, path.Destination); }

        /// <summary>
        /// The state to transition into if a path to an unsorted box is found.
        /// </summary>
        protected override State GetNextStateIfNonzeroLengthPathFound() { return new MoveToBoxState(context, path); }

        /// <summary>
        /// The state to transition into if no path to any of the unsorted boxes is found.
        /// </summary>
        protected override State GetNextStateIfNoPathFound() { return new StuckState(context); }

        /// <summary>
        /// Constructs a Pathfinding state from a set of unsorted box locations.
        /// </summary>
        public FindPathToBoxState(StateContext context, List<Vector2Int> unsortedBoxes) : base(context, unsortedBoxes) { }

    }
}
