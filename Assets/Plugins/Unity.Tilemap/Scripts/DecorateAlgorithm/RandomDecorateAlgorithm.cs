using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Tilemaps
{
    public class RandomDecorateAlgorithm : TilemapDecorateAlgorithm
    {
        public TileType tileType;
        public float fillRate = 0.5f;


        public override void Generate(TilemapCreator creator, int layer, TilemapData map, DecorateConfig decorateConfig)
        {
            if (!decorateConfig.prefab)
                return;
            List<Vector2Int> grids = new List<Vector2Int>();

            grids.AddRange(map.FindTiles(tileType));

            int total = (int)(fillRate * grids.Count);

            for (int i = 0; i < total && grids.Count > 0; i++)
            {
                int index = Random.Range(0, grids.Count);
                Vector2Int grid = grids[index];
                grids.RemoveAt(index);
                var tileFlags = map.GetTileFlags(grid.x, grid.y);
                CreateDecorateObject(creator, decorateConfig, layer, grid.x, grid.y, tileFlags);
            }
        }


    }
}