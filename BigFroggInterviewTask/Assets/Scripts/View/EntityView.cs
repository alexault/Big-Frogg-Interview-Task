using BigFroggInterviewTask.Model;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    public abstract class EntityView : MonoBehaviour
    {
        public enum EntityTilemapLayer
        {
            Base = 0,
            Equipment = 1,
            Icon = 2,
        };

        public abstract TileBase GetTile(EntityModel model, EntityTilemapLayer layer);
    }
}