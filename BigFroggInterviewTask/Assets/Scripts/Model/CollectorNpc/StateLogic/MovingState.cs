using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model.CollectorNpc;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents any state where the NPC is moving along a path toward a destination.
    /// </summary>
    public abstract class MovingState : TimerState
    {
        /// <summary>
        /// The state to transition into if the NPC reaches the destination.
        /// </summary>
        protected abstract State NextStateIfDestinationReached { get; }

        /// <summary>
        /// The state to transition into if the path is interrupted.
        /// </summary>
        protected abstract State NextStateIfPathInterrupted { get; }

        /// <summary>
        /// The set of potential destinations. The shortest path to any destination in the list will be found.
        /// </summary>
        protected Path path;

        /// <summary>
        /// Constructs a Timer state.
        /// </summary>
        public MovingState(StateContext context, Path path) : base(context)
        {
            this.path = path;
        }

        /// <summary>
        /// Complete the action the NPC takes for this state when the timer expires.
        /// </summary>
        protected override ProcessResult TimerExpired(WorldModel world, Vector2Int npcLocation)
        {
            if (path.IsPathPassable(world))
            {
                Vector2Int nextStep = path.PopNextStep();
                world.MoveEntity(world.GetEntityAt(npcLocation), npcLocation, nextStep);

                if (path.StepsRemaining == 0)
                {
                    Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC reached destination [{path.Destination}]");
                    context.State = NextStateIfDestinationReached;
                }
                else
                {
                    Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC continuing path to [{path.Destination}], {path.StepsRemaining} steps remaining");
                }

                // Movement always requires one world tick.
                return ProcessResult.ProcessComplete;
            }
            else
            {
                // Transition immediately into new pathfinding state.
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC path interrupted, finding new path");
                context.State = NextStateIfPathInterrupted;
                return ProcessResult.ProcessNextState;
            }
        }
    }
}