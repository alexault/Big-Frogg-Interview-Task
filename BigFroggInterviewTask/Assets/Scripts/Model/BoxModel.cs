using System;
using UnityEngine;

namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// The model for the boxes.
    /// </summary>
    public class BoxModel : EntityModel
    {
        /// <summary>
        /// Enumerates the possible box colors
        /// </summary>
        public enum BoxColor
        {
            None,
            Blue,
            Red,
        };

        /// <summary>
        /// The set of configuration data required to create a box.
        /// </summary>
        public struct Configuration
        {
            public BoxColor Color;
        };

        /// <summary>
        /// The entity name.
        /// </summary>
        public override string Name { get { return $"{Color} Box"; } }

        /// <summary>
        /// The color of the box.
        /// </summary>
        public BoxColor Color { get; }

        /// <summary>
        /// Creates a new box.
        /// </summary>
        /// <param name="config">The configuration data used to create the box.</param>
        public BoxModel(Configuration config)
        {
            if (config.Color == BoxColor.None)
            {
                throw new ArgumentException("BoxModel: Box not assigned a color", "config.Color");
            }

            Color = config.Color;
        }

        /// <summary>
        /// Run the entity's logic and update its state.
        /// </summary>
        public override void Update(WorldModel world, Vector2Int location)
        {
            // Boxes do not react to anything in the world and so contain no logic
        }
    }
}