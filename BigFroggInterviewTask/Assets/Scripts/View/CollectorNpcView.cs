using BigFroggInterviewTask.Model;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    /// <summary>
    /// The view for collector NPC entities.
    /// </summary>
    public class CollectorNpcView : EntityView
    {
        /// <summary>
        /// The base tile shown for collector NPCs.
        /// </summary>
        [SerializeField]
        private TileBase CollectorBase;

        /// <summary>
        /// The tile shown to indicate that the collector NPC is carrying a blue box.
        /// </summary>
        [SerializeField]
        private TileBase BlueBoxCollected;

        /// <summary>
        /// The tile shown to indicate that the collector NPC is carrying a red box.
        /// </summary>
        [SerializeField]
        private TileBase RedBoxCollected;

        /// <summary>
        /// The tile shown to indicate that the collector NPC is stuck.
        /// </summary>
        [SerializeField]
        private TileBase StuckIcon;

        /// <summary>
        /// Get the tile shown at a given layer for a collector NPC entity based on its model data.
        /// </summary>
        public override TileBase GetTile(EntityModel model, EntityTilemapLayer layer)
        {
            if (!(model is CollectorNpcModel))
            {
                throw new ArgumentException("CollectorNpcView.GetTile: Model is not a CollectorNpcModel", "model");
            }

            CollectorNpcModel collectorNpc = model as CollectorNpcModel;

            switch (layer)
            {
                case EntityTilemapLayer.Base: return CollectorBase;
                case EntityTilemapLayer.Equipment: return GetEquipmentTile(collectorNpc);
                case EntityTilemapLayer.Icon: return GetIconTile(collectorNpc);
                default: return null;
            }
        }

        /// <summary>
        /// Get the tile shown on the entity equipment layer for a collector NPC entity based on its model data.
        /// </summary>
        private TileBase GetEquipmentTile(CollectorNpcModel collectorNpc)
        {
            if (collectorNpc.HasCollectedBox)
            {
                switch (collectorNpc.CollectedBoxColor)
                {
                    case BoxModel.BoxColor.Blue: return BlueBoxCollected;
                    case BoxModel.BoxColor.Red: return RedBoxCollected;
                    default:
                        throw new ArgumentException($"CollectorNpcView.GetEquipmentTile: Unhandled Color in box model");
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the tile shown on the entity icon layer for a collector NPC entity based on its model data.
        /// </summary>
        private TileBase GetIconTile(CollectorNpcModel collectorNpc)
        {
            return collectorNpc.IsStuck ? StuckIcon : null;
        }
    }
}