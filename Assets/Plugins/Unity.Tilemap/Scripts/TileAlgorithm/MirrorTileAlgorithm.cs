using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Unity.Tilemaps
{
    public class MirrorTileAlgorithm : TileAlgorithm
    {
        public int tileWidth = 1;
        public int tileHeight = 1;

        public override List<GameObject> Generate(TilemapCreator creator, Tilemap tilemap, TilemapData map, Transform parent, TilemapLayer layer, TileGroup[] tiles, float scale)
        {
            var tiles2 = tiles.Select(o => o.tile.GetItems().Where(o2 => o2.tileType == TileType.Ground).ToArray()).ToArray();

            int blockWidth = tileWidth * 2;
            int blockHeight = tileHeight * 2;
            Vector3 tileOffset = new Vector3((tileWidth - 1) * 0.5f, 0, (tileHeight - 1) * 0.5f);
            List<GameObject> list = new List<GameObject>();
            Transform t;
            for (int x = 0; x < map.Width; x += blockWidth)
            {
                for (int y = 0; y < map.Height; y += blockHeight)
                {
                    if (map.IsValidPoint(x, y, tileWidth, tileHeight) && !map.HasBlock(x, y, tileWidth, tileHeight))
                    {
                        t = CreateTile(creator, x, y, parent, NextTile(tiles, tiles2), 0f, scale, tileOffset, new Vector3(1, 1, 1));
                        if (t)
                            list.Add(t.gameObject);
                    }
                    if (map.IsValidPoint(x + tileWidth, y, tileWidth, tileHeight) && !map.HasBlock(x + tileWidth, y, tileWidth, tileHeight))
                    {
                        t = CreateTile(creator, x + tileWidth, y, parent, NextTile(tiles, tiles2), 0f, scale, tileOffset, new Vector3(-1, 1, 1));
                        if (t)
                            list.Add(t.gameObject);
                    }
                    if (map.IsValidPoint(x, y + tileHeight, tileWidth, tileHeight) && !map.HasBlock(x, y + tileHeight, tileWidth, tileHeight))
                    {
                        t = CreateTile(creator, x, y + tileHeight, parent, NextTile(tiles, tiles2), 0f, scale, tileOffset, new Vector3(1, 1, -1));
                        if (t)
                            list.Add(t.gameObject);
                    }
                    if (map.IsValidPoint(x + tileWidth, y + tileHeight, tileWidth, tileHeight) && !map.HasBlock(x + tileWidth, y + tileHeight, tileWidth, tileHeight))
                    {
                        t = CreateTile(creator, x + tileWidth, y + tileHeight, parent, NextTile(tiles, tiles2), 0f, scale, tileOffset, new Vector3(-1, 1, -1));
                        if (t)
                            list.Add(t.gameObject);
                    }
                }
            }
            return list;
        }

        public Transform CreateTile(TilemapCreator creator, int x, int y, Transform parent, TileItem tileItem, float offsetAngle, float scale, Vector3 offset, Vector3 scale2)
        {
            Transform t = CreateTileObject(creator, null, x, y, parent, tileItem, offset, new Vector3(0f, offsetAngle, 0f), new Vector3(scale, scale, scale));
            if (t)
            {
                t.localScale = Vector3.Scale(t.localScale, scale2);

            }

            return t;
        }

        TileItem NextTile(TileGroup[] tiles, TileItem[][] tiles2)
        {
            int index = Random.Range(0, tiles.Length);
            var tile = tiles[index].tile;
            var tileItem = tiles2[index][Random.Range(0, tiles2[index].Length)];
            return tileItem;
        }


    }
}