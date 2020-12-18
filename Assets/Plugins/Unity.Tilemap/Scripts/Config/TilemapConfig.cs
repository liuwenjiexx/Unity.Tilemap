using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.Tilemaps
{

    [Serializable]
    public class TilemapConfig
    {
        public int width;
        public int height;
        [Range(0.1f, 10f)]
        public float scale = 1f;

        public TileGroup[] tiles;

        public TilemapLayer[] layers;

         
        public TilemapConfig()
        {
            width = 10;
            height = 10;
        }

    }

     

    [Serializable]
    public class PaintConfig
    {
    }

    public enum DecorateRuleType
    {
        Random,
        Pattern,
    }
    [Serializable]
    public class DecorateItemConfig
    {

    }
    [Serializable]
    public class DecorateRuleConfig
    {

    }


    [Serializable]
    public class DecorateRandomRuleConfig : DecorateRuleConfig
    {
        [Range(0, 1f)]
        public float weight;
        public TileType tileType;
    }

    [Serializable]
    public class DecoratePatternRuleConfig : DecorateRuleConfig
    {
        public TileType tileType;
        public int mode=1;
    }



}