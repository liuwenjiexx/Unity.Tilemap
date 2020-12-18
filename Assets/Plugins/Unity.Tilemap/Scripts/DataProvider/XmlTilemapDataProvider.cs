using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

namespace Unity.Tilemaps
{
    public class XmlTilemapDataProvider : TilemapDataProvider
    {
        public override string FileExtensionName => "xml";

        public override void Read(TilemapCreator creator, TilemapDataSettings options, Stream reader)
        {
            XmlDocument doc = new XmlDocument();
            using (var sr = new StreamReader(reader, Encoding.UTF8))
            {
                doc.Load(sr);
            }
            Tilemap tilemap = creator.Tilemap;
            var mapNode = doc.SelectSingleNode("map");
            if (mapNode == null)
                throw new System.Exception("not found map node");
            tilemap.width = int.Parse(mapNode.Attributes["width"].Value);
            tilemap.height = int.Parse(mapNode.Attributes["height"].Value);

            creator.Maps.Clear();

            int layerIndex = 0;
            foreach (XmlNode layerNode in mapNode.SelectNodes("layer"))
            {
                var dataNode = layerNode.SelectSingleNode("data");
                TilemapData map = new TilemapData(tilemap.width, tilemap.height);
                if (dataNode != null)
                {
                    var values = dataNode.InnerText.Split(',');

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] != "0")
                        {
                            map[i] = TilemapData.BLOCK;
                        }
                    }
                }
                creator.Maps.Add(map);
                layerIndex++;
            }
        }


        public override void Write(TilemapCreator creator, TilemapDataSettings options, Stream writer)
        {
            XmlDocument doc = new XmlDocument();
            var mapNode = doc.CreateElement("map");
            doc.AppendChild(mapNode);
            Tilemap tilemap = creator.Tilemap;
            XmlAttribute attr;
            attr = doc.CreateAttribute("width");
            attr.Value = tilemap.width.ToString();
            mapNode.Attributes.Append(attr);

            attr = doc.CreateAttribute("height");
            attr.Value = tilemap.height.ToString();
            mapNode.Attributes.Append(attr);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tilemap.layers.Length; i++)
            {
                var map = creator.GetMap(i);
                var layerNode = doc.CreateElement("layer");
                {
                    var dataNode = doc.CreateElement("data");
                    {
                        int mapLength = map.Width * map.Height;
                        sb.Clear();
                        for (int j = 0; j < mapLength; j++)
                        {
                            var value = map[j] ? "1" : "0";
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



    }

}