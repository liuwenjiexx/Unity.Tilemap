using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{
    public class FrameAlgorithm : BlockAlgorithm
    {
        [SerializeField]
        public RectOffsetSerializable margin = new();
        [SerializeField]
        public RectOffsetSerializable border = new();
        [SerializeField]
        public RectOffsetSerializable padding = new();
        public bool content = false;


        public override void Generate(TilemapData map, TilemapData mask, out Vector3 startPosition, out Vector3 endPosition)
        {

            startPosition = Vector3.zero;
            endPosition = Vector3.zero;
            int width = map.Width, height = map.Height;
            int minX, maxX, minY, maxY;

            if (!content)
            {
                minX = 0;
                minY = 0;
                maxX = width - 1;
                maxY = height - 1;

            }
            else
            {
                minX = width - 1;
                maxX = 0;
                minY = height - 1;
                maxY = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (mask.IsBlock(x, y))
                        {
                            if (x < minX)
                                minX = x;
                            if (x > maxX)
                                maxX = x;
                            if (y < minY)
                                minY = y;
                            if (y > maxY)
                                maxY = y;
                        }
                    }
                }

                if (minX > maxX)
                    return;
                minX -= border.left;
                minY -= border.bottom;
                maxY += border.top;
                maxX += border.right;
            }


            minX -= padding.left;
            minY -= padding.bottom;
            maxY += padding.top;
            maxX += padding.right;

            if (margin != null)
            {
                minX += margin.left;
                maxX -= margin.right;
                minY += margin.bottom;
                maxY -= margin.top;
            }

            int bWidth = maxX - minX + 1;
            int bHeight = maxY - minY + 1;

            if (bWidth > 0)
            {
                map.Fill(minX, minY, bWidth, border.bottom, TilemapData.BLOCK);
                map.Fill(minX, maxY - border.top + 1, bWidth, border.top, TilemapData.BLOCK);
            }

            if (bHeight > 0)
            {
                map.Fill(minX, minY, border.left, bHeight, TilemapData.BLOCK);
                map.Fill(maxX - border.right + 1, minY, border.right, bHeight, TilemapData.BLOCK);
            }
        }
    }
}
