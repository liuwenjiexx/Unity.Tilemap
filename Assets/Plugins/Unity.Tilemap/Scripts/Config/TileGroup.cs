using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.Tilemaps
{

    [Serializable]
    public class TileGroup
    {
        public Tile tile; 
        public float weight = 1f;
        public int group;

        public TileGroup()
        {
            //this.offset = new Vector3(0.5f, 0.5f, 0.5f); 
        }
         

    }



}