using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{
    /// <summary>
    /// 平铺模式
    /// </summary>
    [Serializable]
    public class TilePattern : IEquatable<TilePattern>
    {
        [SerializeField]
        private TilePatternGrid[] grids;
        public Vector2Int offset;
        public float offsetAngle;

        public int BlockCount { get; private set; }



        public TilePattern()
        {
            this.grids = new TilePatternGrid[0];
        }

        public TilePattern( params TilePatternGrid[] grids)
        {
            this.grids = new TilePatternGrid[grids.Length];
            grids.CopyTo(this.grids, 0);
            BlockCount = 0;
            foreach(var cell in grids)
            {
                if (cell.IsBlock)
                    BlockCount++;
            }
        }

        public TilePatternGrid this[int index]
        {
            get { return grids[index]; }
        }

        public int Length
        {
            get { return grids.Length; }
        }
        static Dictionary<TileType, TilePattern> defaultPattern;

        public static Dictionary<TileType, TilePattern> DefaultPattern
        {
            get
            {
                if (defaultPattern == null)
                {
                    defaultPattern = new Dictionary<TileType, TilePattern>();
                    defaultPattern[TileType.Edge] = new TilePattern(
                        new TilePatternGrid(0, 0, TilePatternType.Block),
                        //new TilePatternGrid(0, 1, TilePatternType.Exclude),
                        //new TilePatternGrid(1, 1, TilePatternType.Exclude),
                        new TilePatternGrid(0, -1, TilePatternType.Include),
                        new TilePatternGrid(-1, 0, TilePatternType.Include),
                        new TilePatternGrid(1, 0, TilePatternType.Include),
                        new TilePatternGrid(-1, -1, TilePatternType.Include),
                        new TilePatternGrid(1, -1, TilePatternType.Include)
                    //new TilePatternGrid(-1, 1, TilePatternType.Exclude)
                    );

                    defaultPattern[TileType.OuterCorner] = new TilePattern(
                        new TilePatternGrid(0, 0, TilePatternType.Block),
                        //new TilePatternGrid(1, 0, TilePatternType.Exclude),
                        //new TilePatternGrid(1, 1, TilePatternType.Exclude),
                        new TilePatternGrid(0, -1, TilePatternType.Include),
                        new TilePatternGrid(-1, 0, TilePatternType.Include),
                        new TilePatternGrid(-1, -1, TilePatternType.Include)
                    //new TilePatternGrid(0, 1, TilePatternType.Exclude)
                    );


                    defaultPattern[TileType.InnerCorner] = new TilePattern(
                        new TilePatternGrid(0, 0, TilePatternType.Block),
                        new TilePatternGrid(0, 1, TilePatternType.Include),
                        //new TilePatternGrid(1, 1, TilePatternType.Exclude),
                        new TilePatternGrid(1, 0, TilePatternType.Include),
                        new TilePatternGrid(1, -1, TilePatternType.Include),
                        new TilePatternGrid(0, -1, TilePatternType.Include),
                        new TilePatternGrid(-1, -1, TilePatternType.Include),
                        new TilePatternGrid(-1, 0, TilePatternType.Include),
                        new TilePatternGrid(-1, 1, TilePatternType.Include)
                    );

                    defaultPattern[TileType.Block] = new TilePattern(
                        new TilePatternGrid(0, 0, TilePatternType.Block),
                        new TilePatternGrid(0, 1, TilePatternType.Include),
                        new TilePatternGrid(1, 1, TilePatternType.Include),
                        new TilePatternGrid(1, 0, TilePatternType.Include),
                        new TilePatternGrid(1, -1, TilePatternType.Include),
                        new TilePatternGrid(0, -1, TilePatternType.Include),
                        new TilePatternGrid(-1, -1, TilePatternType.Include),
                        new TilePatternGrid(-1, 0, TilePatternType.Include),
                        new TilePatternGrid(-1, 1, TilePatternType.Include)
                    );

                    defaultPattern[TileType.Ground] = new TilePattern(
                    new TilePatternGrid(0, 0, TilePatternType.Block)
                );
                }
                return defaultPattern;
            }
        }

        public static readonly TilePattern Empty = new TilePattern();

        public bool Contains(int x, int y)
        {
            return FindIndex(x, y) != -1;
        }

        public int FindCenterIndex()
        {
            return FindIndex(0, 0);
        }

        public int FindIndex(int x, int y)
        {
            int index = -1;
            TilePatternGrid grid;
            for (int i = 0, len = grids.Length; i < len; i++)
            {
                grid = grids[i];
                if (grid.X == x && grid.Y == y)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public void Insert(int index, TilePatternGrid grid)
        {
            var tmp = new TilePatternGrid[grids.Length + 1];
            if (index >= grids.Length)
            {
                Array.Copy(grids, 0, tmp, 0, grids.Length);
                index = grids.Length;
            }
            else
            {
                Array.Copy(grids, 0, tmp, 0, index);
                Array.Copy(grids, index, tmp, index + 1, grids.Length - index);
            }
            tmp[index] = grid;
            this.grids = tmp;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0)
            {
                var tmp = new TilePatternGrid[grids.Length - 1];
                if (index > 0)
                    Array.Copy(grids, 0, tmp, 0, index);
                if (index < grids.Length - 1)
                    Array.Copy(grids, index + 1, tmp, index, grids.Length - index - 1);
                this.grids = tmp;
            }
        }

        public void Set(TilePattern pattern)
        {
            grids = new TilePatternGrid[pattern.Length];
            if (pattern.Length > 0)
            {
                Array.Copy(pattern.grids, grids, pattern.Length);
            }
            this.offset = pattern.offset;
            this.offsetAngle = pattern.offsetAngle;
        }

        public TilePattern Clone()
        {
            TilePattern result = new TilePattern();
            result.Set(this);
            return result;
        }

        public TilePattern Rotate(int offsetX, int offsetY, float angle)
        {
            Quaternion rot = Quaternion.Euler(0, 0, -angle);
            TilePatternGrid[] newGrids = new TilePatternGrid[grids.Length];
            TilePatternGrid grid;
            Vector3 pos = new Vector3();
            int x, y;
            for (int i = 0, len = grids.Length; i < len; i++)
            {
                grid = grids[i];
                pos.x = grid.X + offsetX;
                pos.y = grid.Y + offsetY;
                pos = rot * pos;
                x = Mathf.RoundToInt(pos.x);
                y = Mathf.RoundToInt(pos.y);
                grid = new TilePatternGrid(x, y, grid.Type);
                newGrids[i] = grid;
            }
            var result = new TilePattern(newGrids);
            result.offset = offset + new Vector2Int(offsetX, offsetY);
            result.offsetAngle = offsetAngle + angle;
            return result;
        }


        public bool IsMatch(TilemapData map, RectInt area, int x, int y, bool value)
        {
            var grids = this.grids;
            TilePatternGrid grid;
            int _x, _y;
            bool success = true;
            for (int i = 0, len = grids.Length; i < len; i++)
            {
                grid = grids[i];
                _x = x + grid.X;
                _y = y + grid.Y;

                if (grid.Type == TilePatternType.Block || grid.Type == TilePatternType.Include)
                {
                    if (!map.IsValidPoint(_x, _y) || !area.Contains(new Vector2Int(_x, _y)) || map[_x, _y] != value)
                    {
                        success = false;
                        break;
                    }
                }
                else if (grid.Type == TilePatternType.Exclude)
                {
                    if (map.IsValidPoint(_x, _y) && area.Contains(new Vector2Int(_x, _y)) && map[_x, _y] == value)
                    {
                        success = false;
                        break;
                    }
                }
            }
            return success;
        }

        public bool ExistsBlock(TilemapData map, int x, int y)
        {
            TilePatternGrid grid;
            for (int i = 0, len = Length; i < len; i++)
            {
                grid = grids[i];
                if (grid.IsBlock)
                {
                    if (map.IsBlock(x + grid.X, y + grid.Y))
                        return true;
                }
            }
            return false;
        }
        public void FillBlock(TilemapData map, int x, int y)
        {
            TilePatternGrid grid;
            for (int i = 0, len = Length; i < len; i++)
            {
                grid = grids[i];
                if (grid.IsBlock)
                {
                    map.SetBlock(x + grid.X, y + grid.Y);
                }
            }
        }


        public override bool Equals(object obj)
        {
            if (obj != null && obj is TilePattern)
            {
                return Equals((TilePattern)obj);
            }
            return base.Equals(obj);
        }
        public bool Equals(TilePattern other)
        {
            if (other == null)
                return false;
            if (Length != other.Length)
                return false;
            for (int i = 0, len = Length; i < len; i++)
            {
                if (!grids[i].Equals(other[i]))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            int hashCode = 0;
            if (grids != null)
            {
                for (int i = 0; i < grids.Length; i++)
                    hashCode |= grids[i].GetHashCode();
            }
            return hashCode;
        }
    }



}