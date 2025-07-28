using BigFroggInterviewTask.Model;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    /// <summary>
    /// The view for box entities.
    /// </summary>
    public class BoxView : EntityView
    {
        /// <summary>
        /// The tile shown in the UI for blue boxes.
        /// </summary>
        [SerializeField]
        private TileBase BlueBox;

        /// <summary>
        /// The tile shown in the UI for red boxes.
        /// </summary>
        [SerializeField]
        private TileBase RedBox;

        /// <summary>
        /// Get the tile shown at a given layer for a box entity based on its model data.
        /// </summary>
        public override TileBase GetTile(EntityModel model, EntityTilemapLayer layer)
        {
            if (!(model is BoxModel))
            {
                throw new ArgumentException("BoxView.GetTile: Model is not a BoxModel", "model");
            }

            BoxModel box = model as BoxModel;

            switch (layer)
            {
                case EntityTilemapLayer.Base: return GetBaseTile(box);
                default: return null;
            }
        }

        /// <summary>
        /// Get the tile shown on the base entity layer for a box entity based on its model data.
        /// </summary>
        private TileBase GetBaseTile(BoxModel box)
        {
            switch (box.Color)
            {
                case BoxModel.BoxColor.Blue:
                    return BlueBox;
                case BoxModel.BoxColor.Red:
                    return RedBox;
                default:
                    throw new ArgumentException($"BoxView.GetBaseTile: Unhandled Color in box model");
            }
        }
    }
}