using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{

    /// <summary>
    /// 地图数据
    /// </summary>
    [Serializable]
    public class TilemapData
    {
        [SerializeField]
        private int width;
        [SerializeField]
        private int height;
        [SerializeField]
        private bool[] data;


        public TilemapData(TilemapData other)
            : this(other.Width, other.Height)
        {
            Array.Copy(other.data, data, width * height);
        }

        public TilemapData(int width, int height)
        {
            if (width < 0)
                width = 0;
            if (height < 0)
                height = 0;
            this.width = width;
            this.height = height;
            data = new bool[width * height];
            Clear();
        }

        public TilemapData(int width, int height, bool[] data)
        {
            if (width < 0)
                width = 0;
            if (height < 0)
                height = 0;
            this.width = width;
            this.height = height;
            this.data = data;
        }

        public bool this[int index]
        {
            get { return data[index]; }
            set
            {
                if (index >= 0 && index < data.Length)
                    data[index] = value;
            }
        }

        public bool this[int x, int y]
        {
            get { return data[y * width + x]; }
            set
            {
                if (IsValidPoint(x, y))
                    data[y * width + x] = value;
            }
        }

        public int Width { get => width; }
        public int Height { get => height; }

        public bool[] RawData { get { return data; } }

        public const bool BLOCK = true;

        public static int GridToIndex(int x, int y, int width)
        {
            return y * width + x;
        }
        public static int GridToIndex(Vector2Int grid, int width)
        {
            return grid.y * width + grid.x;
        }
        public static Vector2Int IndexToGrid(int index, int width)
        {
            Vector2Int grid = new Vector2Int(index % width, index / width);
            return grid;
        }

        public bool IsValidPoint(int x, int y)
        {
            if (0 <= x && x < width && 0 <= y && y < height)
                return true;
            return false;
        }
        public bool IsValidPoint(int x, int y, int blockWidth, int blockHeight)
        {
            if (!IsValidPoint(x, y))
                return false;
            return IsValidPoint(x + blockWidth - 1, y + blockHeight - 1);
        }

        public void SetBlock(int x, int y, bool isBlock)
        {
            this[x, y] = isBlock ? BLOCK : !BLOCK;
        }
        public void SetBlock(int x, int y)
        {
            this[x, y] = BLOCK;
        }

        public void SetBlock(int x, int y, int blockWidth, int blockHeight)
        {
            Fill(x, y, blockWidth, blockHeight, BLOCK);
        }

        public void Fill(int x, int y, int blockWidth, int blockHeight, bool value)
        {
            int _x, _y;
            for (int i = 0; i < blockHeight; i++)
            {
                for (int j = 0; j < blockWidth; j++)
                {
                    _x = x + j;
                    _y = y + i;
                    if (!IsValidPoint(_x, _y))
                        continue;
                    this[_x, _y] = value;
                }
            }
        }
        public void FillRect(Matrix4x4 mat, Rect rect, bool value)
        {
            Vector3 point;
            Matrix4x4 inverse = mat.inverse;
            Rect rect2 = rect.MultiplyMatrix(mat);

            int xMin = (int)(rect2.min.x - 0.5f);
            int xMax = (int)(rect2.max.x + 0.5f);
            int yMin = (int)(rect2.min.y - 0.5f);
            int yMax = (int)(rect2.max.y + 0.5f);

            for (int _y = yMin; _y <= yMax; _y++)
            {
                for (int _x = xMin; _x <= xMax; _x++)
                {
                    if (!IsValidPoint(_x, _y))
                        continue;
                    point = inverse.MultiplyPoint(new Vector3(_x, 0, _y));
                    point.x = Mathf.RoundToInt(point.x);
                    point.z = Mathf.RoundToInt(point.z);
                    if (!rect.Contains(new Vector2(point.x, point.z)))
                        continue;
                    this[_x, _y] = value;
                }
            }

        }


        public void Clear()
        {
            for (int i = 0, len = data.Length; i < len; i++)
            {
                data[i] = !BLOCK;
            }
        }

        public void Copy(int x, int y, TilemapData dst, int dstX, int dstY, int blockWidth, int blockHeight)
        {
            Copy(data, x, y, width, dst.data, dstX, dstY, dst.width, blockWidth, blockHeight);
        }

        public static void Copy(bool[] src, int srcX, int srcY, int srcWidth, bool[] dst, int dstX, int dstY, int dstWidth, int blockWidth, int blockHeight)
        {
            int srcLength = src.Length;
            int dstLength = dst.Length;
            int srcIndex, dstIndex;

            for (int y = 0; y < blockHeight; y++)
            {
                for (int x = 0; x < blockWidth; x++)
                {
                    srcIndex = (srcY + y) * srcWidth + (srcX + x);
                    if (srcIndex < 0 || srcIndex >= srcLength)
                        continue;
                    dstIndex = (dstY + y) * dstWidth + (dstX + x);
                    if (dstIndex < 0 || dstIndex >= dstLength)
                        continue;
                    dst[dstIndex] = src[srcIndex];

                }
            }
        }



        public bool IsBlock(int x, int y)
        {
            if (0 <= x && x < width && 0 <= y && y < height)
            {
                if (this[x, y] == BLOCK)
                    return true;
            }
            return false;
        }
        public bool IsBlock(RectInt area, int x, int y)
        {
            if (area.Contains(new Vector2Int(x, y)))
            {
                if (this[x, y] == BLOCK)
                    return true;
            }
            return false;
        }
        public bool IsBlock(int x, int y, int blockWidth, int blockHeight)
        {
            if (blockWidth > 1 || blockHeight > 1)
            {
                for (int i = 0; i < blockWidth; i++)
                {
                    for (int j = 0; j < blockHeight; j++)
                    {
                        if (!IsBlock(x + i, y + j))
                            return false;
                    }
                }
            }
            else
            {
                if (!IsBlock(x, y))
                    return false;
            }
            return true;
        }

        public bool HasBlock(int x, int y, int blockWidth, int blockHeight)
        {
            if (blockWidth > 1 || blockHeight > 1)
            {
                for (int i = 0; i < blockWidth; i++)
                {
                    for (int j = 0; j < blockHeight; j++)
                    {
                        if (IsBlock(x + i, y + j))
                            return true;
                    }
                }
            }
            else
            {
                if (IsBlock(x, y))
                    return true;
            }
            return false;
        }





        public TileFlags GetTileFlags(int x, int y)
        {
            return GetTileFlags(new RectInt(0, 0, width, height), x, y);
        }

        public TileFlags GetTileFlags(RectInt area, int x, int y)
        {
            TileFlags type = 0;
            if (IsBlock(area, x, y - 1))
                type |= TileFlags.Down;
            if (IsBlock(area, x, y + 1))
                type |= TileFlags.Up;
            if (IsBlock(area, x - 1, y))
                type |= TileFlags.Left;
            if (IsBlock(area, x + 1, y))
                type |= TileFlags.Right;
            if (IsBlock(area, x - 1, y - 1))
                type |= TileFlags.DownLeft;
            if (IsBlock(area, x - 1, y + 1))
                type |= TileFlags.UpLeft;
            if (IsBlock(area, x + 1, y - 1))
                type |= TileFlags.DownRight;
            if (IsBlock(area, x + 1, y + 1))
                type |= TileFlags.UpRight;
            type = type.Parse();
            return type;
        }


        public TileType GetTileType(int x, int y)
        {
            var side = GetTileFlags(x, y);
            return side.ToTileType();
        }
        public TileType GetTileType(RectInt area, int x, int y)
        {
            var side = GetTileFlags(area, x, y);
            return side.ToTileType();
        }





        public int GetSide8Count(int x, int y)
        {
            int side = 0;
            if (IsBlock(x, y - 1))
                side++;
            if (IsBlock(x, y + 1))
                side++;
            if (IsBlock(x - 1, y))
                side++;
            if (IsBlock(x + 1, y))
                side++;
            if (IsBlock(x - 1, y - 1))
                side++;
            if (IsBlock(x - 1, y + 1))
                side++;
            if (IsBlock(x + 1, y - 1))
                side++;
            if (IsBlock(x + 1, y + 1))
                side++;
            return side;
        }

        public int GetSide4Count(int x, int y)
        {
            int side = 0;
            if (IsBlock(x, y - 1))
                side++;
            if (IsBlock(x, y + 1))
                side++;
            if (IsBlock(x - 1, y))
                side++;
            if (IsBlock(x + 1, y))
                side++;

            return side;
        }

        public void ClipSide8(int x, int y, int blockWidth, int blockHeight, int sideLimit, int totalThreshold)
        {
            int clipCount = 0;
            int n = 0;
            do
            {
                n = 0;
                for (int i = 0; i < blockHeight; i++)
                {
                    for (int j = 0; j < blockWidth; j++)
                    {
                        int _x = (x + j);
                        int _y = y + i;
                        if (IsBlock(_x, _y))
                        {
                            int side = GetSide8Count(_x, _y);
                            if (side < sideLimit)
                            {
                                this[_x, _y] = !BLOCK;
                                n++;
                                clipCount++;
                            }
                        }
                    }
                }
                //if (clipCount > totalThreshold)
                //    break;
            } while (n > 0);
        }
        public int GetBlockCount()
        {
            int count = 0;
            for (int i = 0, len = width * height; i < len; i++)
            {
                if (data[i] == BLOCK)
                    count++;
            }
            return count;
        }
        public void ClipSide4(int x, int y, int blockWidth, int blockHeight, int sideLimit, int totalThreshold)
        {
            int clipCount = 0;
            int n = 0;
            do
            {
                n = 0;
                for (int i = 0; i < blockHeight; i++)
                {
                    for (int j = 0; j < blockWidth; j++)
                    {
                        int _x = (x + j);
                        int _y = y + i;
                        if (IsBlock(_x, _y))
                        {
                            int side = GetSide4Count(_x, _y);
                            if (side < sideLimit)
                            {
                                this[_x, _y] = !BLOCK;
                                n++;
                                clipCount++;
                            }
                        }
                    }
                }
                //if (clipCount > totalThreshold)
                //    break;
            } while (n > 0);
        }

        public void ClipInvalidTileType(TilemapData mask)
        {
            ClipInvalidTileType(0, 0, width, height, mask);
        }

        public void ClipInvalidTileType(int x, int y, int blockWidth, int blockHeight, TilemapData mask)
        {
            TileFlags sideTileType;
            TileType tileType;
            int n = 0;


            ClipMarginBlock(x, y, blockWidth, blockHeight, mask);
            RectInt area = new RectInt(0, 0, width, height);
            do
            {
                n = 0;
                for (int i = 0; i < blockHeight; i++)
                {
                    for (int j = 0; j < blockWidth; j++)
                    {
                        int _x = (x + j);
                        int _y = y + i;
                        if (IsBlock(_x, _y))
                        {
                            TileFlags sideType = GetTileFlags(_x, _y);
                            tileType = sideType.ToTileType();
                            if (tileType == TileType.Ground)
                            {
                                SetBlock(_x, _y, false);
                                n++;
                                continue;
                            }


                            if (HasMarginBlock(_x, _y))
                            {
                                SetBlock(_x, _y, false);
                                n++;
                                continue;
                            }


                            if (tileType == TileType.OuterCorner)
                            {
                                if (_x % 2 == 1 || _y % 2 == 1)
                                {
                                    //Debug.Log(_x + ", " + _y + ", " + sideTileType + "," + tileType);
                                    //SetBlock(_x, _y, false);
                                    //n++;
                                    //continue;
                                }

                            }

                            //if (sideTileType == TileSideType.OuterCornerUpRight)
                            //{

                            //    if (IsSideTileType(sideType, TileSideType.OuterCornerDownLeft))
                            //    {
                            //        this[_x, _y] = !BLOCK;
                            //        n++;
                            //        continue;
                            //    }
                            //}
                            //else if (sideTileType == TileSideType.OuterCornerUpLeft)
                            //{
                            //    if (IsSideTileType(sideType, TileSideType.OuterCornerDownRight))
                            //    {
                            //        this[_x, _y] = !BLOCK;
                            //        n++;
                            //        continue;
                            //    }
                            //}
                            bool success = false;
                            foreach (var pattern in TilemapCreator.AllDefaultPatterns())
                            {
                                if (pattern.IsMatch(this, area, _x, _y, TilemapData.BLOCK))
                                {
                                    success = true;
                                    break;
                                }
                            }
                            if (!success)
                            {

                                this[_x, _y] = !BLOCK;
                                n++;
                                continue;
                            }
                        }
                    }
                }
            } while (n > 0);

        }

        public void ClipMarginBlock(TilemapData mask, int size = 2)
        {
            ClipMarginBlock(0, 0, width, height, mask, size);
        }

        public void ClipMarginBlock(int x, int y, int blockWidth, int blockHeight, TilemapData mask, int size = 2)
        {

            for (int i = 0; i < blockHeight; i++)
            {
                for (int j = 0; j < blockWidth; j++)
                {
                    int _x = (x + j);
                    int _y = y + i;
                    if (IsBlock(_x, _y) && HasMarginBlock(_x, _y, mask, size))
                        SetBlock(_x, _y, false);
                }
            }
        }


        public bool HasMarginBlock(int x, int y, TilemapData mask, int space = 2)
        {

            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0)
                        continue;
                    if (HasMarginBlock(x, y, ox, oy, mask, space))
                        return true;
                }
            }
            return false;
        }

        public bool HasMarginBlock(int x, int y, int offsetX, int offsetY, TilemapData mask, int space)
        {
            for (int i = 1; i <= space; i++)
            {
                int _x = x + offsetX * i;
                int _y = y + offsetY * i;

                if (mask.IsBlock(_x, _y))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasMarginBlock(int x, int y, int space = 2)
        {

            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0)
                        continue;
                    if (HasMarginBlock(x, y, ox, oy, space))
                        return true;
                }
            }
            return false;
        }
        public bool HasMarginBlock(int x, int y, int offsetX, int offsetY, int space)
        {

            if (IsBlock(x + offsetX, y + offsetY))
                return false;

            for (int i = 2; i <= space; i++)
            {
                int _x = x + offsetX * i;
                int _y = y + offsetY * i;

                if (IsBlock(_x, _y))
                {
                    return true;
                }
            }
            return false;
        }
        public void ClipMarginBlock(int size = 2)
        {
            ClipMarginBlock(0, 0, width, height, size);
        }

        public void ClipMarginBlock(int x, int y, int blockWidth, int blockHeight, int size = 2)
        {

            for (int i = 0; i < blockHeight; i++)
            {
                for (int j = 0; j < blockWidth; j++)
                {
                    int _x = (x + j);
                    int _y = y + i;
                    if (IsBlock(_x, _y))
                    {
                        if (HasMarginBlock(_x, _y, size))
                        {
                            SetBlock(_x, _y, false);
                        }
                    }
                }
            }
        }

        public void AliginBlock(int aliginWidth, int aliginHeight, bool value)
        {
            AliginBlock(0, 0, width, height, aliginWidth, aliginHeight, value);
        }

        public void AliginBlock(int x, int y, int blockWidth, int blockHeight, int aliginWidth, int aliginHeight, bool value)
        {
            int _x, _y;

            if (aliginWidth < 1)
                aliginWidth = 1;
            if (aliginHeight < 1)
                aliginHeight = 1;

            if (aliginWidth == 1 && aliginHeight == 1)
                return;

            for (int i = 0; i < blockWidth; i += aliginWidth)
            {
                for (int j = 0; j < blockHeight; j += aliginHeight)
                {
                    _x = (x + i);
                    _y = y + j;
                    if (HasBlock(_x, _y, aliginWidth, aliginHeight))
                    {
                        if (!IsBlock(_x, _y, aliginWidth, aliginHeight))
                        {
                            Fill(_x, _y, aliginWidth, aliginHeight, value);
                        }
                    }
                }
            }
        }

        public bool TestValidBlock(int x, int y, int blockWidth, int blockHeight, int space = 2)
        {
            int _x, _y;
            int testX, testY;
            for (int i = 0; i < blockWidth; i++)
            {
                for (int j = 0; j < blockHeight; j++)
                {
                    _x = x + i;
                    _y = y + j;

                    if (!IsValidPoint(_x, _y))
                        return false;

                    if (IsBlock(_x, _y))
                        return false;

                    for (int ox = -1; ox <= 1; ox++)
                    {
                        for (int oy = -1; oy <= 1; oy++)
                        {
                            testX = _x + ox;
                            testY = _y + oy;

                            if ((x <= testX && testX <= x + blockWidth - 1) &&
                                (y <= testY && testY <= y + blockHeight - 1))
                                continue;

                            for (int k = 0; k < space; k++)
                            {
                                if (IsBlock(testX, testY))
                                {
                                    //允许相邻
                                    if (k == 0)
                                        break;
                                    return false;
                                }
                                testX += ox;
                                testY += oy;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void Set(TilemapData source, TilemapOperator @operator)
        {
            switch (@operator)
            {
                case TilemapOperator.Copy:
                    source.Copy(0, 0, this, 0, 0, source.Width, source.Height);
                    break;
                case TilemapOperator.And:
                    for (int x = 0, width = source.Width; x < width; x++)
                    {
                        for (int y = 0, height = source.Height; y < height; y++)
                        {
                            if (source.IsBlock(x, y) && IsBlock(x, y))
                            {
                                SetBlock(x, y);
                            }
                            else
                            {
                                SetBlock(x, y, false);
                            }
                        }
                    }
                    break;
                case TilemapOperator.Or:
                    for (int x = 0, width = source.Width; x < width; x++)
                    {
                        for (int y = 0, height = source.Height; y < height; y++)
                        {
                            if (source.IsBlock(x, y))
                            {
                                SetBlock(x, y);
                            }
                        }
                    }
                    break;
                case TilemapOperator.Not:
                    for (int x = 0, width = source.Width; x < width; x++)
                    {
                        for (int y = 0, height = source.Height; y < height; y++)
                        {
                            if (source.IsBlock(x, y))
                            {
                                SetBlock(x, y, false);
                            }
                            else
                            {
                                SetBlock(x, y, true);
                            }
                        }
                    }
                    break;
                case TilemapOperator.Xor:
                    for (int x = 0, width = source.Width; x < width; x++)
                    {
                        for (int y = 0, height = source.Height; y < height; y++)
                        {
                            if (source.IsBlock(x, y) == IsBlock(x, y))
                            {
                                SetBlock(x, y, false);
                            }
                            else
                            {
                                SetBlock(x, y, true);
                            }
                        }
                    }
                    break;
            }
        }

        public IEnumerable<Vector2Int> FindTiles(TileType tileType)
        {
            return FindTiles(0, 0, width, height, tileType);
        }

        public IEnumerable<Vector2Int> FindTiles(int x, int y, int blockWidth, int blockHeight, TileType tileType)
        {
            for (int _x = x; _x < blockWidth; _x++)
            {
                for (int _y = y; _y < blockHeight; _y++)
                {
                    if (GetTileType(_x, _y) == tileType)
                    {
                        yield return new Vector2Int(_x, _y);
                    }
                }
            }
        }


        public IEnumerable<IEnumerable<Vector2Int>> EnumerateClosedBlocks()
        {
            return EnumerateClosedBlocks(0, 0, width, height);
        }
        public IEnumerable<IEnumerable<Vector2Int>> EnumerateClosedBlocks(int x, int y, int blockWidth, int blockHeight)
        {
            RectInt blockArea = new RectInt(x, y, blockWidth, blockHeight);
            int[] mark = new int[width * height];
            int nextId = 1;
            for (int i = 0; i < blockWidth; i++)
            {
                for (int j = 0; j < blockHeight; j++)
                {

                    int _x = x + i;
                    int _y = y + j;
                    int index = _x + _y * width;
                    if (IsBlock(blockArea, _x, _y) && mark[index] == 0)
                    {
                        yield return EnumerateClosedBlocks(new Vector2Int(_x, _y), blockArea, mark, nextId);
                        nextId++;
                    }
                }
            }
        }
        public IEnumerable<IEnumerable<Vector2Int>> EnumerateClosedBlocks(int x, int y, int blockWidth, int blockHeight, int stepWidth, int stepHeight)
        {
            RectInt blockArea = new RectInt(x, y, blockWidth, blockHeight);
            int[] mark = new int[width * height];
            int nextId = 1;
            RectInt area;
            for (int i = 0; i < blockWidth; i += stepWidth)
            {
                for (int j = 0; j < blockHeight; j += stepHeight)
                {
                    int _x = x + i;
                    int _y = y + j;
                    area = new RectInt(_x, _y, stepWidth, stepHeight);
                    if (area.xMax > blockArea.xMax)
                        area.xMax = blockArea.xMax;
                    if (area.yMax > blockArea.yMax)
                        area.yMax = blockArea.yMax;
                    int index = _x + _y * width;
                    if (IsBlock(area, _x, _y) && mark[index] == 0)
                    {
                        yield return EnumerateClosedBlocks(new Vector2Int(_x, _y), area, mark, nextId);
                        nextId++;
                    }
                }
            }
        }

        static Vector2Int[] unionOffsets = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        IEnumerable<Vector2Int> EnumerateClosedBlocks(Vector2Int pos, RectInt blockArea, int[] mark, int blockId)
        {
            int index;
            for (int i = 0; i < unionOffsets.Length; i++)
            {
                Vector2Int _pos = pos + unionOffsets[i];
                if (IsBlock(blockArea, _pos.x, _pos.y))
                {
                    index = _pos.y * width + _pos.x;
                    if (mark[index] == 0)
                    {
                        mark[index] = blockId;
                        yield return _pos;
                        foreach (var next in EnumerateClosedBlocks(_pos, blockArea, mark, blockId))
                        {
                            yield return next;
                        }
                    }
                }
            }

        }
        bool IsClosedBlock(int x, int y, RectInt blockArea, RectInt unitArea)
        {
            if (IsBlock(blockArea, x, y) && unitArea.Contains(new Vector2Int(x, y)))
                return true;
            return false;
        }

        public void ScaleTo(TilemapData dst)
        {
            int dstWidth = dst.width, dstHeight = dst.height;
            float xScale = dstWidth / width;
            float yScale = dstHeight / height;
            bool block;
            for (int x = 0; x < dstWidth; x++)
            {
                for (int y = 0; y < dstHeight; y++)
                {
                    block = IsBlock((int)(x / xScale), (int)(y / yScale));
                    for (int i = 0; i < xScale; i++)
                    {
                        for (int j = 0; j < yScale; j++)
                        {
                            dst.SetBlock(x + i, y + j, block);
                        }
                    }
                }
            }
        }
    }


    //public enum TileSideType2
    //{
    //    None = 0,
    //    AllSide4 = TileSideType.Up | TileSideType.Right | TileSideType.Down | TileSideType.Left,
    //    AllSide = TileSideType.Up | TileSideType.Right | TileSideType.Down | TileSideType.Left | TileSideType.UpRight | TileSideType.UpLeft | TileSideType.DownRight | TileSideType.DownLeft,
    //    Block = TileSideType.Up | TileSideType.Right | TileSideType.Down | TileSideType.Left | TileSideType.UpRight | TileSideType.UpLeft | TileSideType.DownRight | TileSideType.DownLeft,
    //    EdgeUp = TileSideType.Down | TileSideType.Left | TileSideType.Right | TileSideType.DownLeft | TileSideType.DownRight,
    //    EdgeDown = TileSideType.Up | TileSideType.Left | TileSideType.Right | TileSideType.UpLeft | TileSideType.UpRight,
    //    EdgeRight = TileSideType.Up | TileSideType.Down | TileSideType.Left | TileSideType.UpLeft | TileSideType.DownLeft,
    //    EdgeLeft = TileSideType.Up | TileSideType.Down | TileSideType.Right | TileSideType.UpRight | TileSideType.DownRight,
    //    OuterCornerUpRight = TileSideType.Down | TileSideType.Left | TileSideType.DownLeft,
    //    OuterCornerUpLeft = TileSideType.Down | TileSideType.Right | TileSideType.DownRight,
    //    OuterCornerDownRight = TileSideType.Up | TileSideType.Left | TileSideType.UpLeft,
    //    OuterCornerDownLeft = TileSideType.Up | TileSideType.Right | TileSideType.UpRight,
    //    InnerCornerUpRight = TileSideType.Up | TileSideType.UpRight | TileSideType.Right | TileSideType.DownRight | TileSideType.Down | TileSideType.Left | TileSideType.UpLeft,
    //    InnerCornerUpLeft = TileSideType.Up | TileSideType.UpRight | TileSideType.Right | TileSideType.Down | TileSideType.DownLeft | TileSideType.Left | TileSideType.UpLeft,
    //    InnerCornerDownRight = TileSideType.Up | TileSideType.UpRight | TileSideType.Right | TileSideType.DownRight | TileSideType.Down | TileSideType.DownLeft | TileSideType.Left,
    //    InnerCornerDownLeft = TileSideType.Up | TileSideType.Right | TileSideType.DownRight | TileSideType.Down | TileSideType.DownLeft | TileSideType.Left | TileSideType.UpLeft,
    //}

}