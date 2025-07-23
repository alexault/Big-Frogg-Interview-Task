using UnityEngine;

namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// Base class for all entities that can exist in the world.
    /// </summary>
    public abstract class EntityModel
    {
        /// <summary>
        /// The entity name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Run the entity's logic and update its state.
        /// </summary>
        public virtual void Update(WorldModel world, Vector2Int location) { }
    }
}
