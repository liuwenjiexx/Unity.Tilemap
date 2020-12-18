using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;
using System;
using System.Linq;


namespace Unity.Tilemaps
{
    public class ObjectNameDataProvider : TilemapDataProvider
    {
        public override string FileExtensionName
        {
            get
            {
                if (format == TilemapDataFormat.Xml)
                    return "xml";
                return "json";
            }
        }

        public TilemapDataFormat format = TilemapDataFormat.Json;
        public TilemapDataValueFormat valueFormat = TilemapDataValueFormat.String;
        //public bool includeDecorate;



        public override void Read(TilemapCreator creator, TilemapDataSettings options, Stream reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(TilemapCreator creator, TilemapDataSettings options, Stream writer)
        {
            if (format == TilemapDataFormat.Xml)
            {
                WriteXmlData(creator, options, writer);
            }
            else
            {
                WriteJsonData(creator, options, writer);
            }
        }

        List<string[]> GetStringOutputData(TilemapCreator creator, TilemapDataSettings options)
        {
            Tilemap tilemap = creator.Tilemap;
            int width = tilemap.width;
            int height = tilemap.height;
            List<string[]> outLayers = new List<string[]>();

            string[] outLayer = new string[width * height];
            Vector2Int grid;
            for (int i = 0; i < outLayer.Length; i++)
                outLayer[i] = "0";
            outLayers.Add(outLayer);
            for (int layerIndex = 0; layerIndex < tilemap.layers.Length; layerIndex++)
            {
                var tileRoot = creator.GetTilesRoot(layerIndex);
                var map = creator.GetMap(layerIndex);
                for (int i = 0; i < tileRoot.childCount; i++)
                {
                    var item = tileRoot.GetChild(i);
                    grid = creator.WorldPositionToGrid(item.position);
                    if (!map.IsBlock(grid.x, grid.y))
                        continue;
                    int mapIndex = TilemapData.GridToIndex(grid, width);
                    if (mapIndex >= 0 && mapIndex < outLayer.Length)
                        outLayer[mapIndex] = item.name;
                }
                //if (includeDecorate)
                //{
                //    var docurateRoot = creator.GetDecoratesRoot(layerIndex);
                //    for (int i = 0; i < docurateRoot.childCount; i++)
                //    {
                //        var item = docurateRoot.GetChild(i);
                //        grid = creator.WorldPositionToGrid(item.position);
                //        int mapIndex = TilemapData.GridToIndex(grid, width);
                //        if (mapIndex >= 0 && mapIndex < outputLayer.Length)
                //            outputLayer[mapIndex] = item.name;
                //    }
                //}
            }
            return outLayers;
        }


        void WriteJsonData(TilemapCreator creator, TilemapDataSettings options, Stream writer)
        {
            Tilemap tilemap = creator.Tilemap;
            int width = tilemap.width;
            int height = tilemap.height;
            var outLayers = GetStringOutputData(creator, options);
            if (valueFormat == TilemapDataValueFormat.Integer)
            {
                MapIntData data = new MapIntData();
                data.width = width;
                data.height = height;
                data.layers = new LayerIntData[outLayers.Count];
                for (int i = 0; i < outLayers.Count; i++)
                {
                    data.layers[i] = new LayerIntData()
                    {
                        data = outLayers[i].Select(o => int.Parse(o)).ToArray()
                    };
                }
                using (var sw = new StreamWriter(writer))
                {
                    sw.Write(JsonUtility.ToJson(data));
                }
            }
            else
            {
                MapStringData data = new MapStringData();
                data.width = width;
                data.height = height;
                data.layers = new LayerStringData[outLayers.Count];
                for (int i = 0; i < outLayers.Count; i++)
                {
                    data.layers[i] = new LayerStringData()
                    {
                        data = outLayers[i].Select(o => o).ToArray()
                    };
                }
                using (var sw = new StreamWriter(writer))
                {
                    sw.Write(JsonUtility.ToJson(data));
                }
            }

        }


        void WriteXmlData(TilemapCreator creator, TilemapDataSettings options, Stream writer)
        {
            var outLayers = GetStringOutputData(creator, options);

            XmlDocument doc = new XmlDocument();
            var mapNode = doc.CreateElement("map");
            doc.AppendChild(mapNode);
            Tilemap tilemap = creator.Tilemap;
            int width = tilemap.width;
            int height = tilemap.height;
            XmlAttribute attr;
            attr = doc.CreateAttribute("width");
            attr.Value = tilemap.width.ToString();
            mapNode.Attributes.Append(attr);

            attr = doc.CreateAttribute("height");
            attr.Value = tilemap.height.ToString();
            mapNode.Attributes.Append(attr);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < outLayers.Count; i++)
            {
                var map = outLayers[i];
                var layerNode = doc.CreateElement("layer");
                {
                    var dataNode = doc.CreateElement("data");
                    {

                        sb.Clear();
                        for (int j = 0; j < map.Length; j++)
                        {
                            var value = map[j];
                            if (j > 0)
                                sb.Append(',');
                            sb.Append(value);
                        }
                        dataNode.InnerText = sb.ToString();
                    }
                    layerNode.AppendChild(dataNode);
                }
                mapNode.AppendChild(layerNode);
            }

            using (var sw = new StreamWriter(writer, Encoding.UTF8))
            {
                doc.Save(sw);
            }
        }

        [Serializable]
        class MapIntData
        {
            public int width;
            public int height;
            public LayerIntData[] layers;
        }

        [Serializable]
        class LayerIntData
        {
            [SerializeField]
            public int[] data;
        }

        [Serializable]
        class MapStringData
        {
            public int width;
            public int height;
            public LayerStringData[] layers;
        }

        [Serializable]
        class LayerStringData
        {
            [SerializeField]
            public string[] data;
        }

    }



}


public enum TilemapDataFormat
{
    Json,
    Xml
}
public enum TilemapDataValueFormat
{
    String,
    Integer
} 