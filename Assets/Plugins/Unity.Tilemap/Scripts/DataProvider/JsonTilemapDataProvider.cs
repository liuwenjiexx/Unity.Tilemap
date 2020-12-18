using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Unity.Tilemaps
{
    public class JsonTilemapDataProvider : TilemapDataProvider
    {

        public override string FileExtensionName => "json";

        public override void Read(TilemapCreator creator, TilemapDataSettings options, Stream reader)
        {
            using (var sr = new StreamReader(reader, System.Text.Encoding.UTF8))
            {
                var jsonData = JsonUtility.FromJson<MapByteData>(sr.ReadToEnd());
                int mapWidth = jsonData.width;
                int mapHeight = jsonData.height;
                creator.Tilemap.width = mapWidth;
                creator.Tilemap.height = mapHeight;
                creator.Maps.Clear();
                for (int i = 0; i < jsonData.layers.Length; i++)
                {
                    var layerData = jsonData.layers[i];
                    var map = new TilemapData(mapWidth, mapHeight, layerData.data.Select(o => o != 0 ? true : false).ToArray());
                    creator.Maps.Add(map);
                }
            }
        }

        public override void Write(TilemapCreator creator, TilemapDataSettings options, Stream writer)
        {

            int layerCount = creator.Tilemap.layers.Length;
            MapByteData data = new MapByteData();
            data.width = creator.Width;
            data.height = creator.Height;
            data.layers = new LayerByteData[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                var map = creator.GetMap(i);
                var layerData = new LayerByteData();
                layerData.data = map.RawData.Select(o => (byte)(o ? 1 : 0)).ToArray();
                data.layers[i] = layerData;
            }
            using (var sw = new StreamWriter(writer))
            {
                sw.Write(JsonUtility.ToJson(data));
            }
        }


        [Serializable]
        class MapByteData
        {
            public int width;
            public int height;
            public LayerByteData[] layers;
        }

        [Serializable]
        class LayerByteData
        {
            [SerializeField]
            public byte[] data;
        }

    }




}