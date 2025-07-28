using BigFroggInterviewTask.Model.CollectorNpc;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is moving to an unsorted box.
    /// </summary>
    public class MoveToBoxState : MovingState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "MoveToBox"; } }

        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected override int TimerSetpoint { get { return context.TicksPerStepWithoutBox; } }

        /// <summary>
        /// The state to transition into if the NPC reaches the destination.
        /// </summary>
        protected override State NextStateIfDestinationReached { get { return new CollectBoxState(context, path.Destination); }
 }
        /// <summary>
        /// The state to transition into if the path is interrupted.
        /// </summary>
        protected override State NextStateIfPathInterrupted { get { return new FindPathToBoxState(context); }
 }
        /// <summary>
        /// Constructs a Pathfinding state from a set of unsorted box locations.
        /// </summary>
        public MoveToBoxState(StateContext context, Path pathToBox) : base(context, pathToBox) { }

    }
}
