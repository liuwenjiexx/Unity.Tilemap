using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Unity.Tilemaps
{
    //[CreateAssetMenu(fileName = "Tilemap", menuName = "Tilemap/Tilemap")]
    public class Tilemap : ScriptableObject
    {
        public string id;
        public int width;
        public int height;
        [Range(0.1f, 10f)]
        public float scale = 1f;

        public TileGroup[] tiles;

        public TilemapLayer[] layers;
         

        public TilemapDataSettings data;

    }




}
