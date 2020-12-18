using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.Tilemaps
{
    [Serializable]
    public class RandomAlgorithm : BlockAlgorithm
    {
        public float fillRate = 0.5f;
        public int blockWidth = 2;
        public int blockHeight = 2;
        public RectOffsetSerializable margin;

        public override void Generate(TilemapData map, TilemapData mask, out Vector3 startPosition, out Vector3 endPosition)
        {
            startPosition = Vector3.zero;
            endPosition = Vector3.zero;

            List<Vector2Int> blocks = new List<Vector2Int>();
            int xMin = 0, yMin = 0, xMax = map.Width - 1, yMax = map.Height - 1;
            
            for (int _x = 0; _x < map.Width; _x += blockWidth)
            {
                if (margin != null && (_x < margin.left || _x + blockWidth > map.Width - margin.right))
                    continue;
                for (int _y = 0; _y < map.Height; _y += blockWidth)
                {
                    if (margin != null && (_y < margin.bottom || _y + blockHeight > map.Height - margin.top))
                        continue;
                    if (mask.TestValidBlock(_x, _y, blockWidth, blockHeight))
                    {
                        blocks.Add(new Vector2Int(_x, _y));
                    }
                }
            }
            int totalBlock = Mathf.CeilToInt(Mathf.Clamp01(fillRate) * blocks.Count);

            //Debug.Log(blocks.Count + ", " + totalBlock);
            for (int i = 0; i < totalBlock && blocks.Count > 0; i++)
            {
                int index = (int)(Random.value * blocks.Count);
                var block = blocks[index];
                blocks.RemoveAt(index);
                map.SetBlock(block.x, block.y, blockWidth, blockHeight);
            }
            
            //int totalBlock = (int)(Mathf.Clamp01(fillRate) * (((map.Width / blockWidth) * (map.Height / blockHeight))));
            //int blockCount = 0;
            //for (int i = 0; i < totalBlock; i++)
            //{
            //    int x = (int)(Random.value * map.Width);
            //    int y = (int)(Random.value * map.Height);
            //    x -= x % blockWidth;
            //    y -= y % blockHeight;

            //    if (IsMapBorder(map, x, y))
            //        continue;

            //    if (map.IsValidPoint(x, y, blockWidth, blockHeight))
            //    {
            //        if (!mask.HasBlock(x, y, blockWidth, blockHeight) && !map.HasBlock(x, y, blockWidth, blockHeight))
            //        {
            //            map.SetBlock(x, y, blockWidth, blockHeight);
            //            blockCount++;
            //        }
            //    }
            //} 
        }
        bool IsMapBorder(TilemapData map, int x, int y)
        {
            if (x < 2 || x >= map.Width - 2)
                return true;
            if (y < 2 || y >= map.Height - 2)
                return true;
            return false;
        }



    }
}