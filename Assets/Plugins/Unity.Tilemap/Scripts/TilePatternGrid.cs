using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unity.Tilemaps
{

    /// <summary>
    /// 平铺模式的单元格
    /// </summary>
    [Serializable]
    public class TilePatternGrid : IEquatable<TilePatternGrid>
    {
        [SerializeField]
        private int x;
        [SerializeField]
        private int y;
        [SerializeField]
        private TilePatternType type;

        public TilePatternGrid()
        {
        }

        public TilePatternGrid(int x, int y, TilePatternType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }

        public int X { get => x; }
        public int Y { get => y; }
        public TilePatternType Type { get => type; }

        public bool IsBlock { get => type == TilePatternType.Block; }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is TilePatternGrid)
            {
                return Equals((TilePatternGrid)obj);
            }
            return base.Equals(obj);
        }

        public bool Equals(TilePatternGrid other)
        {
            if (other == null)
                return false;

            if (x == other.x && y == other.y && type == other.type)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() | y.GetHashCode() | type.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("({0},{1}), {2}", x, y, type);
        }
    }
}
