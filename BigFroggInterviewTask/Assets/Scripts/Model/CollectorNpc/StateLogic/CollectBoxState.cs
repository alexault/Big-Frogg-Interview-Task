using BigFroggInterviewTask.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// Represents the state where the NPC is collecting an unsorted box.
    /// </summary>
    public class CollectBoxState : TimerState
    {
        /// <summary>
        /// The state name.
        /// </summary>
        public override string Name { get { return "CollectBox"; } }

        /// <summary>
        /// The fixed number of ticks it takes to complete the action.
        /// </summary>
        protected override int TimerSetpoint { get { return context.TicksToCollectBox; } }

        /// <summary>
        /// The location of the box to be collected.
        /// </summary>
        private readonly Vector2Int BoxLocation;

        /// <summary>
        /// Constructs a CollectBox state from the location of a box.
        /// </summary>
        public CollectBoxState(StateContext context, Vector2Int boxLocation) : base(context)
        {
            BoxLocation = boxLocation;
        }

        /// <summary>
        /// Check that a box exists at the target location and collect it if so.
        /// </summary>
        protected override ProcessResult TimerExpired(StateContext context, WorldModel world, Vector2Int npcLocation)
        {
            EntityModel entityAtTarget = world.GetEntityAt(BoxLocation);
            if (entityAtTarget is BoxModel)
            {
                BoxModel box = entityAtTarget as BoxModel;
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC collecting {box.Color} box from [{BoxLocation}]");

                // Collect box and remove it from the world
                context.Box = box;
                world.RemoveEntity(box, BoxLocation);

                context.State = new FindPathToDropoffState(context);
                return ProcessResult.ProcessComplete;
            }
            else
            {
                // Potential AI improvement: collector could check for box disappearing during MoveToBoxState also
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC target box disappeared from [{BoxLocation}], returning to idle");
                context.State = new IdleState(context);
                return ProcessResult.ProcessComplete;
            }
        }
    }
}