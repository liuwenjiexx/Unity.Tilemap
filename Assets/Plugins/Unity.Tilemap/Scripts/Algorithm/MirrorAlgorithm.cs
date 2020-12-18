using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Unity.Tilemaps
{
    public class MirrorAlgorithm : BlockAlgorithm
    {

        public float fillRate = 0.5f;
        public MirrorType mirrorType = MirrorType.Mirror;
        public int blockWidth = 2;
        public int blockHeight = 2; 

        public enum MirrorType
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Mirror = 3,
        }


        public override void Generate(TilemapData map, TilemapData mask, out Vector3 startPosition, out Vector3 endPosition)
        {
            startPosition = Vector3.zero;
            endPosition = Vector3.zero;

            var build = new RandomAlgorithm();
            build.fillRate = fillRate;
            build.blockWidth = blockWidth;
            build.blockHeight = blockWidth; 
            build.Generate(map, mask, out startPosition, out endPosition);

            //float blockRate = fillRate;
            //int blockWidth;
            //int blockHeight;
            //GetTemplateSize(map, out blockWidth, out blockHeight);
            //if (blockRate > 1f)
            //    blockRate = 1f;

            //int totalBlock = (int)(blockRate * (blockWidth * blockHeight));

            //int blockCount = 0;
            //for (int j = 0; j < totalBlock; j++)
            //{
            //    int x = Random.Range(0, blockWidth);
            //    int y = Random.Range(0, blockHeight);
            //    if (IsMapBorder(map, x, y))
            //        continue;
            //    if (map[x, y] != TilemapData.BLOCK)
            //    {
            //        map[x, y] = TilemapData.BLOCK;
            //        blockCount++;
            //    }
            //}

            CopyTemplate(map);

            for (int i = 0, len = map.Width * map.Height; i < len; i++)
            {
                if (mask[i] == TilemapData.BLOCK)
                {
                    if (map[i] == TilemapData.BLOCK)
                        map[i] = !TilemapData.BLOCK;
                }
            }

            map.ClipInvalidTileType(mask);
            if (blockWidth > 1 || blockHeight > 1)
                map.AliginBlock(blockWidth, blockHeight, !TilemapData.BLOCK);

        }
        public void GetTemplateSize(TilemapData map, out int blockWidth, out int blockHeight)
        {
            blockWidth = map.Width;
            blockHeight = map.Height;
            if (mirrorType == MirrorType.Vertical || mirrorType == MirrorType.Mirror)
            {
                blockHeight = (int)(map.Height * 0.5f);
            }
            if (mirrorType == MirrorType.Horizontal || mirrorType == MirrorType.Mirror)
            {
                blockWidth = (int)(map.Width * 0.5f);
            }

        }

        public void CopyTemplate(TilemapData map)
        {
            int width = map.Width;
            int height = map.Height;

            switch (mirrorType)
            {
                case MirrorType.Mirror:
                    {
                        int halfWidth = width / 2;
                        int halfHeight = height / 2;
                        //右
                        map.Copy(0, 0, map, halfWidth, 0, halfWidth, halfHeight);
                        Transform(map, halfWidth, 0, width, halfWidth, halfHeight, MirrorType.Horizontal);
                        //上
                        map.Copy(0, 0, map, 0, halfHeight, halfWidth, halfHeight);
                        Transform(map, 0, halfHeight, width, halfWidth, halfHeight, MirrorType.Vertical);
                        //右上
                        map.Copy(0, 0, map, halfWidth, halfHeight, halfWidth, halfHeight);
                        Transform(map, halfWidth, halfHeight, width, halfWidth, halfHeight, MirrorType.Mirror);
                    }
                    break;
                case MirrorType.Vertical:
                    {
                        int halfHeight = height / 2;
                        map.Copy(0, 0, map, 0, halfHeight, width, halfHeight);
                        Transform(map, 0, halfHeight, width, width, halfHeight, mirrorType);
                    }
                    break;
                case MirrorType.Horizontal:
                    {
                        int halfWidth = width / 2;
                        map.Copy(0, 0, map, halfWidth, 0, halfWidth, height);
                        Transform(map, halfWidth, 0, width, halfWidth, height, mirrorType);
                    }
                    break;
            }
        }

        public static void Transform(TilemapData map, int x, int y, int width, int blockWidth, int blockHeight, MirrorType transform)
        {
            switch (transform)
            {
                case MirrorType.Horizontal:
                    {
                        int srcIndex;
                        int dstIndex;
                        int halfWidth = (int)(blockWidth * 0.5f);
                        int srcLength = map.Width * map.Height;
                        bool tmp;
                        for (int i = 0; i < blockHeight; i++)
                            for (int j = 0; j < halfWidth; j++)
                            {
                                srcIndex = (y + i) * width + (x + j);
                                if (srcIndex < 0 || srcIndex >= srcLength)
                                    continue;
                                dstIndex = (y + i) * width + (x + blockWidth - 1 - j);
                                if (dstIndex < 0 || dstIndex >= srcLength)
                                    continue;
                                tmp = map[srcIndex];
                                map[srcIndex] = map[dstIndex];
                                map[dstIndex] = tmp;
                            }
                    }
                    break;
                case MirrorType.Vertical:
                    {
                        int srcIndex;
                        int dstIndex;
                        int halfHeight = (int)(blockHeight * 0.5f);
                        int srcLength = map.Width * map.Height;
                        bool tmp;
                        for (int i = 0; i < halfHeight; i++)
                            for (int j = 0; j < blockWidth; j++)
                            {
                                srcIndex = (y + i) * width + (x + j);
                                if (srcIndex < 0 || srcIndex >= srcLength)
                                    continue;
                                dstIndex = (y + blockHeight - 1 - i) * width + (x + j);
                                if (dstIndex < 0 || dstIndex >= srcLength)
                                    continue;
                                tmp = map[srcIndex];
                                map[srcIndex] = map[dstIndex];
                                map[dstIndex] = tmp;
                            }
                    }
                    break;
                case MirrorType.Mirror:
                    {

                        Transform(map, x, y, width, blockWidth, blockHeight, MirrorType.Horizontal);
                        Transform(map, x, y, width, blockWidth, blockHeight, MirrorType.Vertical);
                    }
                    break;
            }
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