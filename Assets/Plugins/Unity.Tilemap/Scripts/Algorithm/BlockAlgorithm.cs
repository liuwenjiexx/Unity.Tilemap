using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.Tilemaps
{
    [Serializable]
    public class BlockAlgorithm
    {
        public virtual void Generate(TilemapData map , TilemapData mask, out Vector3 startPosition, out Vector3 endPosition)
        {
            startPosition = Vector3.zero;
            endPosition = Vector3.zero; 
        }

    }
}