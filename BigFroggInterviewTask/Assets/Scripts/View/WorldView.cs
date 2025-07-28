using BigFroggInterviewTask.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    public class WorldView : MonoBehaviour
    {
        /// <summary>
        /// The parent transform of all visible world objects.
        /// </summary>
        [SerializeField]
        private Transform worldSpace;

        /// <summary>
        /// The tilemap representing the world floor.
        /// </summary>
        [SerializeField]
        private Tilemap floorTilemap;

        /// <summary>
        /// The tilemaps representing entities in the world.
        /// </summary>
        [SerializeField]
        private List<Tilemap> entityTilemaps;

        /// <summary>
        /// The camera displaying the world.
        /// </summary>
        [SerializeField]
        private Camera mainCamera;

        /// <summary>
        /// The tile shown in the UI for the floor.
        /// </summary>
        [SerializeField]
        private TileBase FloorTile;

        /// <summary>
        /// The map of entity types to views.
        /// </summary>
        private Dictionary<Type, EntityView> entityViews;

        /// <summary>
        /// The current screen size.
        /// </summary>
        private Vector2Int screenSize;

        /// <summary>
        /// Prepare the world view for use when the game starts.
        /// </summary>
        public void InitializeView(WorldModel model, Dictionary<Type, EntityView> entityViews)
        {
            if (entityTilemaps.Count < Enum.GetNames(typeof(EntityView.EntityTilemapLayer)).Length)
            {
                throw new Exception("WorldView does not have a tilemap to match each layer in EntityTilemapLayer enum");
            }

            this.entityViews = new Dictionary<Type, EntityView>(entityViews);

            // Adjust the position of the world space to put (0, 0) in the bottom left corner
            worldSpace.position = new Vector3(-model.Size.x / 2.0f, -model.Size.y / 2.0f);

            UpdateScreenSize(model.Size);

            DrawFloorTiles(model);
        }

        /// <summary>
        /// Update the world view to reflect any changes in the underlying model.
        /// </summary>
        public void UpdateView(WorldModel model)
        {
            // Check if the screen size has changed
            if (screenSize.x != Screen.width || screenSize.y != Screen.height)
            {
                UpdateScreenSize(model.Size);
            }

            // Redraw the entities
            DrawEntityTiles(model);
        }

        /// <summary>
        /// Draw the floor tiles to the screen.
        /// </summary>
        private void DrawFloorTiles(WorldModel model)
        {
            for (int x = 0; x < model.Size.x; x++)
            {
                for (int y = 0; y < model.Size.y; y++)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y), FloorTile);
                }
            }
        }

        /// <summary>
        /// Draw the entity tiles to the screen.
        /// </summary>
        private void DrawEntityTiles(WorldModel model)
        {
            foreach (EntityView.EntityTilemapLayer layer in Enum.GetValues(typeof(EntityView.EntityTilemapLayer)))
            {
                Tilemap entityTilemap = entityTilemaps[(int)layer];

                entityTilemap.ClearAllTiles();

                for (int x = 0; x < model.Size.x; x++)
                {
                    for (int y = 0; y < model.Size.y; y++)
                    {
                        EntityModel entity = model.GetEntityAt(new Vector2Int(x, y));
                        TileBase tile = null;

                        if (entity != null)
                        {
                            Type entityType = entity.GetType();
                            if (entityViews.ContainsKey(entityType))
                            {
                                tile = entityViews[entityType].GetTile(entity, layer);
                            }
                            else
                            {
                                throw new Exception($"DrawEntityTiles: Unhandled entity type [{entityType}]");
                            }
                        }

                        if (tile != null)
                        {
                            entityTilemap.SetTile(new Vector3Int(x, y), tile);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ensure the entire world fits within the visible area on the screen.
        /// </summary>
        private void UpdateScreenSize(Vector2Int worldSize)
        {
            screenSize = new Vector2Int(Screen.width, Screen.height);

            float screenAspectRatio = (float)screenSize.x / (float)screenSize.y;
            float worldAspectRatio = (float)worldSize.x / (float)worldSize.y;

            if (worldAspectRatio <= screenAspectRatio)
            {
                mainCamera.orthographicSize = worldSize.y / 2.0f;
            }
            else
            {
                mainCamera.orthographicSize = worldSize.x / screenAspectRatio / 2.0f;
            }
        }
    }
}