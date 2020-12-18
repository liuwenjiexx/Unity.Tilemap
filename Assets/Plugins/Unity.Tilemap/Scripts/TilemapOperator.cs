using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{

    public enum TilemapOperator
    {
        /// <summary>
        /// 不操作
        /// </summary>
        None,
        /// <summary>
        /// 源
        /// </summary>
        Copy,
        /// <summary>
        ///  与
        /// </summary>
        And,
        /// <summary>
        /// 或
        /// </summary>
        Or,
        /// <summary>
        ///  非
        /// </summary>
        Not,
        /// <summary>
        ///  异或
        /// </summary>
        Xor,
    }

}