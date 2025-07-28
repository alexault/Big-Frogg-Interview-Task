using BigFroggInterviewTask.Model;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    public class CollectorNpcView : EntityView
    {
        [SerializeField]
        private TileBase CollectorBase;

        [SerializeField]
        private TileBase BlueBoxCollected;

        [SerializeField]
        private TileBase RedBoxCollected;

        [SerializeField]
        private TileBase StuckIcon;

        public override TileBase GetTile(EntityModel model, EntityTilemapLayer layer)
        {
            if (!(model is CollectorNpcModel))
            {
                throw new ArgumentException("CollectorNpcView.GetTile: Model is not a CollectorNpcModel", "model");
            }

            CollectorNpcModel collectorNpc = model as CollectorNpcModel;

            switch (layer)
            {
                case EntityTilemapLayer.Base: return GetBaseTile(collectorNpc);
                case EntityTilemapLayer.Equipment: return GetEquipmentTile(collectorNpc);
                case EntityTilemapLayer.Icon: return GetIconTile(collectorNpc);
                default: return null;
            }
        }

        private TileBase GetBaseTile(CollectorNpcModel collectorNpc)
        {
            return CollectorBase;
        }

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

        private TileBase GetIconTile(CollectorNpcModel collectorNpc)
        {
            return collectorNpc.IsStuck ? StuckIcon : null;
        }
    }
}