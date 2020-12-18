using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{
    [Serializable]
    public class DecorateConfig: ISerializationCallbackReceiver
    {
        public bool enabled = true;
        public int group;
        public int layer;


        public GameObject prefab;
        public Vector3 offset;
        public Vector3 offsetRotation;


        public float fillRate = 0.5f;

        public bool useTileRotation;
        public bool useRandomOffset;
        public Vector3 minRandomOffset;
        public Vector3 maxRandomOffset;
        public bool useRandomRotation;
        public Vector3 minRandomRotation;
        public Vector3 maxRandomRotation;

        public bool useRandomScale;
        //public bool uniformScale;
        public float minRandomScale;
        public float maxRandomScale;

        /// <summary>
        /// 装饰填充算法
        /// </summary>
        public TilemapDecorateAlgorithm algorithm;
        [HideInInspector]
        [SerializeField]
        private SerializableObject _algorithm;
        public void OnBeforeSerialize()
        {
            if (_algorithm == null)
                _algorithm = new SerializableObject();
            _algorithm.Value = algorithm;
        }

        public void OnAfterDeserialize()
        {
            if (_algorithm != null)
                algorithm = _algorithm.Value as TilemapDecorateAlgorithm;
            else
                algorithm = null;
        }
    }

}