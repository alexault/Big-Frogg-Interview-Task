namespace BigFroggInterviewTask.Model
{
    /// <summary>
    /// Base class for all objects that can spawn entities into the world.
    /// </summary>
    public abstract class EntitySpawnerModel
    {
        /// <summary>
        /// Run the spawner logic and spawn entities if conditions are met.
        /// </summary>
        public abstract void Update(WorldModel world);
    }
}