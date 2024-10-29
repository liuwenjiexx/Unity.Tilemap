using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Tilemaps.Example
{

    public class DynamicGenerate : MonoBehaviour
    {
        private TilemapCreator tilemapCreator;
 
        public string cellNameFormat;

        // Start is called before the first frame update
        void Start()
        {
            tilemapCreator = GetComponent<TilemapCreator>();
            Generate();
        }

        public void Generate()
        {
            tilemapCreator.Clear(); 
            int[] layerValues;
            var mapData = GenerateData(tilemapCreator, out layerValues);
            tilemapCreator.LoadLayerValueData(mapData, 2, layerValues);
            tilemapCreator.BuildTileObject();
        }
       

        private static int[] GenerateData(TilemapCreator tilemapCreator, out int[] layerValues)
        {
            int tileBlockSize = 2;
            int row = (int)(tilemapCreator.Width / tileBlockSize);
            int col = (int)(tilemapCreator.Height / tileBlockSize);
            int[] map = new int[row * col];
            layerValues = new int[tilemapCreator.Tilemap.layers.Length];

            for (int i = 0; i < layerValues.Length; i++)
            {
                layerValues[i] = i;
            }

            for (int i = 0; i < map.Length; i++)
            {
                map[i] = layerValues[Random.Range(0, layerValues.Length)];
            }
            return map;
        }


        private void OnGUI()
        {
            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }


    }
}