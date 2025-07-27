using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model.CollectorNpc;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents any state where the NPC is attempting to find the shortest path leading to any of a set of destinations.
    /// </summary>
    public abstract class PathfindingState : State
    {
        /// <summary>
        /// The state to transition into if the NPC is already adjacent to one of the destinations.
        /// </summary>
        protected abstract State GetNextStateIfZeroLengthPathFound();

        /// <summary>
        /// The state to transition into if a path to one of the destinations is found.
        /// </summary>
        protected abstract State GetNextStateIfNonzeroLengthPathFound();

        /// <summary>
        /// The state to transition into if no path to any of the destinations is found.
        /// </summary>
        protected abstract State GetNextStateIfNoPathFound();

        /// <summary>
        /// The set of potential destinations. The shortest path to any destination in the list will be found.
        /// </summary>
        private readonly List<Vector2Int> PotentialDestinations;

        /// <summary>
        /// The shortest path found to any destination.
        /// </summary>
        protected Path path;

        /// <summary>
        /// Constructs a Pathfinding state from a set of destinations.
        /// </summary>
        public PathfindingState(StateContext context, List<Vector2Int> potentialDestinations) : base(context)
        {
            PotentialDestinations = new List<Vector2Int>(potentialDestinations);
        }

        /// <summary>
        /// Attempt to find the shortest path to any of the set of destinations and advance to the next state based on the result.
        /// </summary>
        public override ProcessResult ProcessTick(StateContext context, WorldModel world, Vector2Int npcLocation)
        {
            path = Path.FindShortestPath(world, npcLocation, PotentialDestinations);

            if (path != null)
            {
                if (path.StepsRemaining == 0)
                {
                    Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC at [{npcLocation}] found adjacent [{world.GetEntityAt(path.Destination)}] at [{path.Destination}]");
                    context.State = GetNextStateIfZeroLengthPathFound();
                }
                else
                {
                    Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC at [{npcLocation}] found path of length [{path.StepsRemaining}] to [{world.GetEntityAt(path.Destination)}] at [{path.Destination}]");
                    context.State = GetNextStateIfNonzeroLengthPathFound();
                }
            }
            else
            {
                // Potential AI improvement: collector could check whether any path can be created by moving boxes out of the way.
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC found no path to destination");
                context.State = GetNextStateIfNoPathFound();
            }

            // Pathfinding is always a transitional state that does not require any ticks to complete.
            return ProcessResult.ProcessNextState;
        }
    }
}
