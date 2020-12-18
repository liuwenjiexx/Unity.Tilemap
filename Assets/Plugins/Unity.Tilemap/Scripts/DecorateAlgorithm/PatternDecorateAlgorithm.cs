using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Tilemaps
{
    public class PatternDecorateAlgorithm : TilemapDecorateAlgorithm
    {
        public TileType tileType;
        public int spacing;


        public override void Generate(TilemapCreator creator, int layer, TilemapData map, DecorateConfig decorateConfig)
        {
            if (!decorateConfig.prefab)
                return;

            List<Vector2Int> grids = new List<Vector2Int>();

            grids.AddRange(map.FindTiles(tileType));



            for (int i = 0; i < grids.Count; i++)
            {
                if (spacing > 0 && (i % (spacing + 1)) != 0)
                    continue;
                Vector2Int grid = grids[i];
                var tileFlags = map.GetTileFlags(grid.x, grid.y);

                CreateDecorateObject(creator, decorateConfig, layer, grid.x, grid.y, tileFlags);
            }
        }


    }
}