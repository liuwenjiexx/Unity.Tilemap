using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{

    [Serializable]
    public class TilemapDataSettings : ISerializationCallbackReceiver
    {
        public TilemapDataProvider provider;
        [HideInInspector]
        [SerializeField]
        private SerializableObject _provider;

        public void OnBeforeSerialize()
        {
            if (_provider == null)
                _provider = new SerializableObject();
            _provider.Value = provider;
        }

        public void OnAfterDeserialize()
        {
            if (_provider != null)
                provider = _provider.Value as TilemapDataProvider;
            else
                provider = null;
        }
    }

}