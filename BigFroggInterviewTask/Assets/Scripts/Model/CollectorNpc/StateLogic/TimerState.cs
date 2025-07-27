using BigFroggInterviewTask.Logging;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents any state where the NPC is performing an action that takes a fixed number of world ticks.
    /// </summary>
    public abstract class TimerState : State
    {
        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected abstract int TimerSetpoint { get; }

        /// <summary>
        /// The number of ticks remaining before the action is complete.
        /// </summary>
        private int timerTicksRemaining;

        /// <summary>
        /// Constructs a Timer state.
        /// </summary>
        public TimerState(StateContext context) : base(context)
        {
            timerTicksRemaining = TimerSetpoint;
        }

        /// <summary>
        /// Determine if the timer has expired and if so, process the action.
        /// </summary>
        public override ProcessResult ProcessTick(StateContext context, WorldModel world, Vector2Int npcLocation)
        {
            timerTicksRemaining--;
            if (timerTicksRemaining == 0)
            {
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC timer expired, processing state");

                // If we remain in the same state, the timer restarts.
                timerTicksRemaining = TimerSetpoint;

                // Process the action that occurs when the timer expires.
                return TimerExpired(context, world, npcLocation);
            }
            else
            {
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC waiting on timed state, {timerTicksRemaining} ticks remaining");
                return ProcessResult.ProcessComplete;
            }
        }

        /// <summary>
        /// Complete the action the NPC takes for this state when the timer expires.
        /// </summary>
        protected abstract ProcessResult TimerExpired(StateContext context, WorldModel world, Vector2Int npcLocation);
    }
}