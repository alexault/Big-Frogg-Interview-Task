using BigFroggInterviewTask.Logging;
using BigFroggInterviewTask.Model.CollectorNpc;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigFroggInterviewTask.Model.StateLogic
{
    /// <summary>
    /// The state machine executing collector NPC behavior.
    /// </summary>
    public class StateContext
    {
        /// <summary>
        /// The current state of the collector NPC.
        /// </summary>
        public State State { private get; set; }

        /// <summary>
        /// The rules used to determine the NPC's behavior.
        /// </summary>
        public RuleHandler.RuleSet Rules { get; }

        /// <summary>
        /// The number of world ticks between each step when the NPC is moving without a box.
        /// </summary>
        public int TicksPerStepWithoutBox { get; }

        /// <summary>
        /// The number of world ticks between each step when the NPC is moving while carrying a box.
        /// </summary>
        public int TicksPerStepWithBox { get; }

        /// <summary>
        /// The number of world ticks it takes the NPC to collect an adjacent box.
        /// </summary>
        public int TicksToCollectBox { get; }

        /// <summary>
        /// The number of world ticks it takes the NPC to drop a box it is carrying.
        /// </summary>
        public int TicksToDropBox { get; }

        /// <summary>
        /// Indicates whether the state model has reached a point where it is unable to progress to any other state.
        /// </summary>
        public bool IsStuck { get { return State is StuckState; } }

        /// <summary>
        /// Create a new state machine for the collector NPC based on the given configuration.
        /// </summary>
        public StateContext(CollectorNpcModel.Configuration config)
        {
            if (!Enum.IsDefined(typeof(RuleHandler.WorldSide), config.BlueBoxSortingSide))
            {
                throw new ArgumentOutOfRangeException("config.BlueBoxSortingSide");
            }

            if (!Enum.IsDefined(typeof(RuleHandler.WorldSide), config.RedBoxSortingSide))
            {
                throw new ArgumentOutOfRangeException("config.RedBoxSortingSide");
            }

            if (config.TicksPerStepWithoutBox < 1)
            {
                throw new ArgumentOutOfRangeException("config.TicksPerStepWithoutBox");
            }

            if (config.TicksPerStepWithBox < 1)
            {
                throw new ArgumentOutOfRangeException("config.TicksPerStepWithBox");
            }

            if (config.TicksToCollectBox < 1)
            {
                throw new ArgumentOutOfRangeException("config.TicksToCollectBox");
            }

            if (config.TicksToDropBox < 1)
            {
                throw new ArgumentOutOfRangeException("config.TicksToDropBox");
            }

            Rules = new RuleHandler.RuleSet()
            {
                SortingRules = new Dictionary<BoxModel.BoxColor, RuleHandler.WorldSide>
                {
                    { BoxModel.BoxColor.Blue, config.BlueBoxSortingSide},
                    { BoxModel.BoxColor.Red, config.RedBoxSortingSide},
                }
            };

            TicksPerStepWithoutBox = config.TicksPerStepWithoutBox;
            TicksPerStepWithBox = config.TicksPerStepWithBox;
            TicksToCollectBox = config.TicksToCollectBox;
            TicksToDropBox = config.TicksToDropBox;
        }

        /// <summary>
        /// Run the state machine for a single world tick.
        /// </summary>
        /// <param name="world">The state of the world the NPC resides in.</param>
        /// <param name="npcLocation">The location of the NPC within that world.</param>
        public void ProcessTick(WorldModel world, Vector2Int npcLocation)
        {
            Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC state model processing one tick");

            State.ProcessResult result = State.ProcessResult.ProcessNextState;
            while (result == State.ProcessResult.ProcessNextState)
            {
                Log.Write(Log.Flag.CollectorNpcModelTrace, $"Collector NPC state model in state [{State.Name}]");
                result = State.ProcessTick(this, world, npcLocation);
            } 
        }
    }
}