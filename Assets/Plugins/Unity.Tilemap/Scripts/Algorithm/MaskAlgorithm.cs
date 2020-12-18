using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Tilemaps
{
    public class MaskAlgorithm : BlockAlgorithm
    {

        public TilemapOperator @operator;

        public override void Generate(TilemapData map, TilemapData mask, out Vector3 startPosition, out Vector3 endPosition)
        {

            startPosition = Vector3.zero;
            endPosition = Vector3.zero;
            int width = map.Width, height = map.Height;
            map.Set(mask, @operator);
        }
    }
}