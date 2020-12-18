using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;


namespace Unity.Tilemaps
{


    public class TilemapEditorSettings : ScriptableObject
    {

        public Color hoverColor = new Color(0, 0, 1, 0.3f);
        public Color gridColor = new Color(1, 1, 1, 0.05f);
        public Color gridCenterColor = new Color(1, 1, 1, 0.1f);
        public bool showGrid = true;
        public bool hideUnselectedLayer = true;

        public bool showClosedBlockNumber;
        public Color groundColor = new Color(0f, 0f, 0f, 0.5f);
        public Color closedBlockNumberColor = new Color(1, 1, 1, 0.5f);


        //const string TilemapEditorSettingsPath = "Assets/Editor/Settings/TilemapEditorSettings.asset";
        //private static TilemapEditorSettings instance;
        //public static TilemapEditorSettings Instance
        //{
        //    get
        //    {
        //        if (!instance)
        //        {

        //        }
        //        return instance;
        //    }
        //}
    }

}