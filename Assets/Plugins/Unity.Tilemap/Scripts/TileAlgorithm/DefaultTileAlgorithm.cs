using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Tilemaps
{
    public class DefaultTileAlgorithm : TileAlgorithm
    {
        public override List<GameObject> Generate(TilemapCreator creator, Tilemap tilemap, TilemapData map, Transform parent, TilemapLayer layer, TileGroup[] tiles, float scale)
        {
            return BuildTile(creator, tilemap, map, parent, layer, tiles, scale);
        }
        public List<GameObject> BuildTile(TilemapCreator creator, Tilemap tilemap, TilemapData map, Transform parent, TilemapLayer layer, TileGroup[] tileGroups, float scale)
        {
            TileGroup tileGroup = null;
            TransformPattern2[] blockPattern = null;
            TransformPattern2[] groundPattern = null;
            if (tileGroups.Length > 0)
            {
                tileGroup = tileGroups[Random.Range(0, tileGroups.Length)];
                bool isBlock = true;
                if ((layer.flags & TilemapLayerFlags.TileInverse) != 0)
                    isBlock = false;

                blockPattern = GetPattern(tileGroup.tile, isBlock);
                groundPattern = GetPattern(tileGroup.tile, !isBlock);

            }
            //Debug.Log("pattern:"+ layer.layerIndex + ", " + blockPattern.Length);
            List<GameObject> list = new List<GameObject>();
            if (tileGroup == null)
                return list;

            TransformPattern2 findPattern;

            var flags = layer.flags;
            int width = map.Width, height = map.Height;
            TileItem tileItem;

            TilemapData excludeBlock = new TilemapData(map.Width, map.Height);
            TilemapData groundMap = new TilemapData(map);
            TilePattern pattern = null;
            RectInt blockArea;
            blockArea = new RectInt(0, 0, map.Width, map.Height);
            if ((flags & TilemapLayerFlags.TileBlock) == TilemapLayerFlags.TileBlock)
            {
                int tileBlockSize = Mathf.Max(layer.tileBlockSize, 1);
                /*  if (tileBlockSize <= 1)
                  {

                      for (int x = 0; x < width; x++)
                      {
                          for (int y = 0; y < height; y++)
                          {
                              if (!map.IsBlock(x, y))
                              {
                                  continue;
                              }
                              findPattern = Find(blockPattern, map, blockArea, x, y, blockMap, out pattern);

                              if (findPattern == null)
                                  continue;

                              if (findPattern.items.Count == 0)
                                  continue;
                              pattern.FillBlock(blockMap, x, y);

                              tileItem = findPattern.items[Random.Range(0, findPattern.items.Count)];
                              CreateTile(x, y, parent, layer, tileItem, pattern.offsetAngle, scale);
                          }
                      }
                  }
                  else
                  {*/
                Vector3 scale3 = Vector3.one * scale;
                //Debug.Log("xxx:" + layer.layerIndex + ", " + tileBlockSize);
                for (int x = 0; x < width; x += tileBlockSize)
                {
                    for (int y = 0; y < height; y += tileBlockSize)
                    {

                        if (tileBlockSize > 1)
                        {
                            blockArea = new RectInt(x, y, tileBlockSize, tileBlockSize);
                        }

                        for (int ox = 0; ox < tileBlockSize; ox++)
                        {
                            for (int oy = 0; oy < tileBlockSize; oy++)
                            {
                                int _x = x + ox;
                                int _y = y + oy;
                                if (!map.IsBlock(_x, _y))
                                {
                                    continue;
                                }
                                findPattern = Find(blockPattern, map, blockArea, _x, _y, excludeBlock, out pattern);

                                if (findPattern == null)
                                    continue;

                                if (findPattern.items.Count == 0)
                                    continue;

                                pattern.FillBlock(excludeBlock, _x, _y);
                                tileItem = findPattern.items[Random.Range(0, findPattern.items.Count)];
                                var t = CreateTileObject(creator, tilemap, _x, _y, parent, tileItem, Vector3.zero, new Vector3(0f, pattern.offsetAngle, 0f), scale3);
                                if (t)
                                    list.Add(t.gameObject);
                            }
                        }

                    }
                }

                //  }

            }
            //debugDrawMap = blockMap;

            if ((flags & TilemapLayerFlags.TileGround) == TilemapLayerFlags.TileGround)
            {
                Vector3 scale3 = Vector3.one * scale;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        findPattern = null;
                        if (map.IsBlock(x, y))
                        {
                            continue;
                        }
                        findPattern = FindGround(groundPattern, excludeBlock, x, y, null, out pattern);

                        if (findPattern == null)
                            continue;
                        if (findPattern.items.Count == 0)
                            continue;
                        pattern.FillBlock(excludeBlock, x, y);
                        tileItem = findPattern.items[Random.Range(0, findPattern.items.Count)];
                        var t = CreateTileObject(creator, tilemap, x, y, parent, tileItem, Vector3.zero, new Vector3(0f, pattern.offsetAngle, 0f), scale3);
                        if (t)
                            list.Add(t.gameObject);

                    }
                }
            }
            return list;
        }
        class TransformPattern2
        {
            public TilePattern[] patterns;
            public List<TileItem> items;
        }


        static TransformPattern2[] GetPattern(Tile tile, bool block)
        {
            if (!tile)
                return new TransformPattern2[0];
            Dictionary<TilePattern, TransformPattern2> dic = new Dictionary<TilePattern, TransformPattern2>();
            TilePattern pattern;
            foreach (var item in tile.GetItems())
            {
                if (block)
                {
                    if (item.tileType == TileType.Ground)
                        continue;
                }
                else
                {
                    if (item.tileType != TileType.Ground)
                        continue;
                }

                pattern = item.GetPattern();
                TransformPattern2 p;
                if (!dic.TryGetValue(pattern, out p))
                {
                    p = new TransformPattern2();
                    if (block)
                    {
                        List<TilePattern> ps = new List<TilePattern>();
                        ps.Add(pattern);
                        for (int i = 0; i < pattern.Length; i++)
                        {
                            if (pattern[i].IsBlock)
                            {
                                var grid = pattern[i];
                                if (!(grid.X == 0 && grid.Y == 0))
                                    continue;
                                Vector2Int offset = new Vector2Int(-grid.X, -grid.Y);
                                if (!(offset.x == 0 && offset.y == 0))
                                    ps.Add(pattern.Rotate(offset.x, offset.y, 0));

                                ps.Add(pattern.Rotate(offset.x, offset.y, 90));
                                ps.Add(pattern.Rotate(offset.x, offset.y, 180));
                                ps.Add(pattern.Rotate(offset.x, offset.y, 270));

                            }
                        }
                        p.patterns = ps.ToArray();
                    }
                    else
                    {
                        p.patterns = new TilePattern[] { pattern };
                    }
                    p.items = new List<TileItem>();
                    dic.Add(pattern, p);
                }
                p.items.Add(item);
            }
            return dic.Values.OrderByDescending(o => o.patterns[0].Length)/*.OrderBy(o =>
            {
                switch (o.items[0].tileType)
                {
                    case TileType.Block:
                        return 0;
                    case TileType.InnerCorner:
                        return 1;
                    case TileType.Edge:
                        return 2;
                    case TileType.OuterCorner:
                        return 3;
                    case TileType.Ground:
                        return 4;
                }
                return 0;
            })*/.ToArray();
        }
        static TransformPattern2 Find(TransformPattern2[] patterns, TilemapData map, RectInt blockArea, int x, int y, TilemapData excludeBlock, out TilePattern pattern2)
        {

            TransformPattern2 findPattern = null;
            TransformPattern2 pattern;
            pattern2 = null;
            TilePattern transformPattern;
            //RectInt area = new RectInt(0, 0, map.Width, map.Height);
            for (int i = 0; i < patterns.Length; i++)
            {
                pattern = patterns[i];
                for (int j = 0; j < pattern.patterns.Length; j++)
                {
                    transformPattern = pattern.patterns[j];
                    if (excludeBlock != null && transformPattern.ExistsBlock(excludeBlock, x, y))
                        continue;
                    if (transformPattern.IsMatch(map, blockArea, x, y, TilemapData.BLOCK))
                    {
                        findPattern = pattern;
                        pattern2 = transformPattern;
                        break;
                    }
                }
                if (findPattern != null)
                    break;
            }
            return findPattern;
        }
        static TransformPattern2 FindGround(TransformPattern2[] patterns, TilemapData map, int x, int y, TilemapData excludeBlock, out TilePattern pattern2)
        {

            TransformPattern2 findPattern = null;
            TransformPattern2 pattern;
            pattern2 = null;
            TilePattern transformPattern;
            RectInt area = new RectInt(0, 0, map.Width, map.Height);
            for (int i = 0; i < patterns.Length; i++)
            {
                pattern = patterns[i];
                for (int j = 0; j < pattern.patterns.Length; j++)
                {
                    transformPattern = pattern.patterns[j];
                    if (excludeBlock != null && transformPattern.ExistsBlock(excludeBlock, x, y))
                        continue;
                    if (transformPattern.IsMatch(map, area, x, y, !TilemapData.BLOCK))
                    {
                        findPattern = pattern;
                        pattern2 = transformPattern;
                        break;
                    }
                }
                if (findPattern != null)
                    break;
            }
            return findPattern;
        }





    }
}