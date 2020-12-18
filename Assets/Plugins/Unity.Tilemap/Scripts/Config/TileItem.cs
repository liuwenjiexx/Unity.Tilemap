using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Unity.Tilemaps
{


    [Serializable]
    public class TileItem
    {
        [SerializeField]
        public TileType tileType;
        [Obsolete]
        public GameObject prefab;
        /// <summary>
        /// 偏移修正，为了兼容其它插件的Tile Prefab 资源
        /// </summary>
        public Vector3 offset;
        /// <summary>
        /// 旋转修正，为了兼容其它插件的Tile Prefab 资源
        /// </summary>
        public Vector3 rotation;
        public bool usePattern;
        public TilePattern pattern;

        [SerializeField]
        public TilePrefab[] items;



        public TilePattern GetPattern()
        {
            TilePattern result = null;
            if (usePattern)
            {
                result = pattern;
            }
            else
            {
                if (TilePattern.DefaultPattern.ContainsKey(tileType))
                    result = TilePattern.DefaultPattern[tileType];
            }
            if (result == null)
                result = TilePattern.Empty;
            return result;
        }

        public bool IsEmpty
        {
            get { return items == null || items.Length == 0; }
        }

        public bool ContainsPrefab(GameObject prefab)
        {
            if (items != null)
                return items.Where(o => o.prefab == prefab).Count() > 0;
            return false;
        }

    }



}
