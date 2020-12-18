using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.Tilemaps
{
    [Flags]
    public enum TileFlags
    {
        None = 0,
        /// <summary>
        /// 上
        /// </summary>
        Up = 1 << 0,
        /// <summary>
        /// 下
        /// </summary>
        Down = 1 << 1,
        /// <summary>
        /// 右
        /// </summary>
        Right = 1 << 2,
        /// <summary>
        /// 左
        /// </summary>
        Left = 1 << 3,
        /// <summary>
        /// 右上
        /// </summary>
        UpRight = 1 << 4,
        /// <summary>
        /// 左上
        /// </summary>
        UpLeft = 1 << 5,
        /// <summary>
        /// 右下
        /// </summary>
        DownRight = 1 << 6,
        /// <summary>
        /// 左下
        /// </summary>
        DownLeft = 1 << 7,
        AllSide4 = Up | Right | Down | Left,
        AllSide = Up | Right | Down | Left | UpRight | UpLeft | DownRight | DownLeft,

        /// <summary>
        /// 边
        /// </summary>
        Edge = 1 << 8,
        /// <summary>
        /// 外角
        /// </summary>
        OuterCorner = 1 << 9,
        /// <summary>
        /// 内角
        /// </summary>
        InnerCorner = 1 << 10,
        /// <summary>
        /// 块
        /// </summary>
        Block = 1 << 11,


        /// <summary>
        /// 边
        /// </summary>
        EdgeUp = Edge | 1 << 12,
        EdgeDown = Edge | 1 << 13,
        EdgeRight = Edge | 1 << 14,
        EdgeLeft = Edge | 1 << 15,
        /// <summary>
        /// 外角
        /// </summary>
        OuterCornerUpRight = OuterCorner | 1 << 16,
        OuterCornerUpLeft = OuterCorner | 1 << 17,
        OuterCornerDownRight = OuterCorner | 1 << 18,
        OuterCornerDownLeft = OuterCorner | 1 << 19,
        /// <summary>
        /// 内角
        /// </summary>
        InnerCornerUpRight = InnerCorner | 1 << 20,
        InnerCornerUpLeft = InnerCorner | 1 << 21,
        InnerCornerDownRight = InnerCorner | 1 << 22,
        InnerCornerDownLeft = InnerCorner | 1 << 23,

        TileFlags = Block | Edge | OuterCorner | InnerCorner

    }

}