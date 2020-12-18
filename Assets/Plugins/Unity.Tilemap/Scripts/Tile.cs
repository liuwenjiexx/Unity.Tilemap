using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.Tilemaps
{
    //[CreateAssetMenu(fileName = "Tile", menuName = "TileMap/Tile")]
    public class Tile : ScriptableObject
    {
        [Obsolete]
        public TileItem[] items;
        public TileItem edge;
        public TileItem outerCorner;
        public TileItem innerCorner;
        public TileItem block;
        public TileItem ground;

 
        private void OnEnable()
        {
            if (edge == null)
                edge = new TileItem() { tileType = TileType.Edge };
            if (block == null)
                block = new TileItem() { tileType = TileType.Block };
            if (innerCorner == null)
                innerCorner = new TileItem() { tileType = TileType.InnerCorner };
            if (outerCorner == null)
                outerCorner = new TileItem() { tileType = TileType.OuterCorner };
            if (ground == null)
                ground = new TileItem() { tileType = TileType.Ground };

        }

        public IEnumerable<TileItem> GetItems()
        {
            if (block != null && !block.IsEmpty)
                yield return block;
            if (innerCorner != null && !innerCorner.IsEmpty)
                yield return innerCorner;
            if (edge != null && !edge.IsEmpty)
                yield return edge;
            if (outerCorner != null && !outerCorner.IsEmpty)
                yield return outerCorner;
            if (ground != null && !ground.IsEmpty)
                yield return ground;
        }


        //public int GetCount(TileType tileType)
        //{
        //    int n = 0;
        //    if (items != null)
        //    {
        //        for (int i = 0; i < items.Length; i++)
        //        {
        //            if (items[i].tileType == tileType)
        //                n++;
        //        }
        //    }
        //    return n;
        //}

        //public int GetTileItem(TileType tileType, out TileItem result)
        //{
        //    int count = GetCount(tileType);
        //    int index2 = -1;
        //    if (count > 0)
        //    {
        //        int index = UnityEngine.Random.Range(0, count);
        //        for (int i = 0, j = 0; i < items.Length; i++)
        //        {
        //            if (items[i].tileType == tileType)
        //            {
        //                if (j == index)
        //                {
        //                    result = items[i];
        //                    return i;
        //                }
        //                j++;
        //            }
        //        }
        //    }
        //    result = new TileItem();
        //    return index2;
        //}



    }


    [Serializable]
    public struct TilePrefab
    {
        public GameObject prefab;
        public float weight;
        public bool rotation;

    }
}