using BigFroggInterviewTask.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BigFroggInterviewTask.View
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField]
        private Transform worldSpace;

        [SerializeField]
        private Tilemap floorTilemap;

        [SerializeField]
        private List<Tilemap> entityTilemaps;

        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private TileBase FloorTile;

        private Dictionary<Type, EntityView> entityViews;

        private Vector2Int screenSize;

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