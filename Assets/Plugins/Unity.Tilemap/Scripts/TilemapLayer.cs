using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{

    [Serializable]
    public class TilemapLayer : ISerializationCallbackReceiver
    {
        /// <summary>
        /// 是否启用该层
        /// </summary>
        public bool enabled;
        /// <summary>
        /// 块生成算法
        /// </summary>
        public BlockAlgorithm algorithm;
        [HideInInspector]
        [SerializeField]
        private SerializableObject _algorithm;

        /// <summary>
        /// Tile生成算法
        /// </summary>
        public TileAlgorithm tileAlgorithm;
        [HideInInspector]
        [SerializeField]
        private SerializableObject _tileAlgorithm;

        public float offsetHeight;
        public bool blockMask;
        public int tileBlockSize = 1;


        public DecorateConfig[] decorates;

        public TilemapInput input = new TilemapInput();
        public TilemapOutput output = new TilemapOutput();
        /// <summary>
        /// 描述
        /// </summary>
        public string note;

        public TilemapLayerFlags flags = TilemapLayerFlags.TileBlock | TilemapLayerFlags.TileGround;

        [NonSerialized]
        [HideInInspector]
        public int layerIndex;

        public int tileGroup;
        public void OnBeforeSerialize()
        {
            if (_algorithm == null)
                _algorithm = new SerializableObject();
            _algorithm.Value = algorithm;
            if (_tileAlgorithm == null)
                _tileAlgorithm = new SerializableObject();
            _tileAlgorithm.Value = tileAlgorithm;
        }

        public void OnAfterDeserialize()
        {
            if (_algorithm != null)
                algorithm = _algorithm.Value as BlockAlgorithm;
            else
                algorithm = null;

            if (_tileAlgorithm != null)
                tileAlgorithm = _tileAlgorithm.Value as TileAlgorithm;
            else
                tileAlgorithm = null;
        }
    }

    [Flags]
    public enum TilemapLayerFlags
    {
        None,
        TileBlock = 1 << 0,
        TileGround = 1 << 1,
        Hide = 1 << 2,
        GroundUseBlock = 1 << 3,
    }
    [Serializable]
    public class TilemapInput
    {
        public MaskOperator mask;
        public TilemapOperator maskToMap;

    }
    [Serializable]
    public class TilemapOutput
    {
        public MaskOperator mask;
        public TilemapOperator mapToMask;

    }

}