using BigFroggInterviewTask.Model;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    /// <summary>
    /// Base class for entity views.
    /// </summary>
    public abstract class EntityView : MonoBehaviour
    {
        /// <summary>
        /// Enumerates the tilemap layers that entities may display tiles on.
        /// </summary>
        public enum EntityTilemapLayer
        {
            Base = 0,
            Equipment = 1,
            Icon = 2,
        };

        /// <summary>
        /// Get the tile shown at a given layer for an entity based on its model data.
        /// </summary>
        public abstract TileBase GetTile(EntityModel model, EntityTilemapLayer layer);
    }
}