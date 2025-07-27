using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents a single state within the collector NPC state machine.
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// Enumerates the possible results from processing a state.
        /// </summary>
        public enum ProcessResult
        {
            ProcessNextState,
            ProcessComplete,
        };

        /// <summary>
        /// The state name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The state machine context.
        /// </summary>
        protected StateContext context;

        /// <summary>
        /// Constructs a state.
        /// </summary>
        public State(StateContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Process the collector NPC logic for this state.
        /// </summary>
        /// <returns>
        /// ProcessNextState if the state executed in zero ticks, and that the next state should be processed within the same tick.
        /// ProcessComplete if the state took one tick to execute, and that the next state should not be processed until the next tick.
        /// </returns>
        public abstract ProcessResult ProcessTick(StateContext context, WorldModel world, Vector2Int npcLocation);
    }
}