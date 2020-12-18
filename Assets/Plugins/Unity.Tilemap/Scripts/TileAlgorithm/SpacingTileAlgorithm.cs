using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Unity.Tilemaps
{

    public class SpacingTileAlgorithm : DefaultTileAlgorithm
    {
        public int interval = 2;
        public float spacing = 0f;
        public TilemapCreator creator;


        public override List<GameObject> Generate(TilemapCreator creator, Tilemap tilemap,TilemapData map, Transform parent, TilemapLayer layer, TileGroup[] tiles, float scale)
        {
            if (!creator)
                creator = GameObject.FindObjectOfType<TilemapCreator>();

            var list = base.Generate(creator,  tilemap, map, parent, layer, tiles, scale);

            foreach (Transform child in list.Select(o => o.transform))
            {
                var grid = creator.WorldPositionToGrid(child.position);

                Vector3 position = child.position;
                int x1 = grid.x / interval, y1 = grid.y / interval;
                position.x += x1 * spacing;
                position.z += y1 * spacing;
                child.position = position;
            }
            return list;
        }



    }
}