using BigFroggInterviewTask.Model;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    public class BoxView : EntityView
    {
        [SerializeField]
        private TileBase BlueBox;

        [SerializeField]
        private TileBase RedBox;

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