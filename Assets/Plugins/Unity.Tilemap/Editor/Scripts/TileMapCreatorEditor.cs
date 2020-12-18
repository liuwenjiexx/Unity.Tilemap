using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEditor.SceneManagement;

namespace Unity.Tilemaps
{

    [CustomEditor(typeof(TilemapCreator))]
    public class TilemapCreatorEditor : Editor
    {
        private const string UnityPackageName = "Unity.TileMap";
        private static string packageDir;
        public Vector3 layerOffset;

        public static string PackageDir
        {
            get
            {
                if (string.IsNullOrEmpty(packageDir))
                {
                    packageDir = GetPackageDirectory(UnityPackageName);
                }
                return packageDir;
            }
        }


        private static string GetPackageDirectory(string packageName)
        {
            foreach (var dir in Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories))
            {
                if (string.Equals(Path.GetFileName(dir), packageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return dir;
                }
            }

            string path = Path.Combine("Packages", packageName);
            if (Directory.Exists(path))
            {
                return path;
            }

            return null;
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        TilemapCreator Creator
        {
            get { return this.target as TilemapCreator; }
        }

        TilemapData selectedMap;

        public static TilemapCreator CreatorInstance;

        private void OnEnable()
        {
            CreatorInstance = Creator;

        }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            if (!Application.isPlaying)
            {
                if (TilemapCreator.EditorInstantiateGameObject == null)
                    TilemapCreator.EditorInstantiateGameObject = InstantiateGameObject;
            }
        }
         

        void DrawGrid()
        {
            var creator = Creator;
            var config = creator.Tilemap;
            int gridWidth = config.width, gridHeight = config.height;
            Vector3 cellSize = new Vector3(config.scale, 0f, config.scale);
            var oldMat = Handles.matrix;
            Handles.matrix = creator.Root.localToWorldMatrix;


            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 startPos = new Vector3(cellSize.x * x, 0f, 0f) + layerOffset;
                Vector3 endPos = new Vector3(cellSize.x * x, 0f, cellSize.z * gridHeight) + layerOffset;
                if (x == (int)(gridWidth * 0.5f))
                {
                    Handles.color = EditorSettings.gridCenterColor;
                }
                else
                {
                    Handles.color = EditorSettings.gridColor;
                }
                Handles.DrawLine(startPos, endPos);
                Handles.color = Color.white;
            }

            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 startPos = new Vector3(0f, 0f, cellSize.z * y) + layerOffset;
                Vector3 endPos = new Vector3(cellSize.x * gridWidth, 0f, cellSize.z * y) + layerOffset;
                if (y == (int)(gridHeight * 0.5f))
                {
                    Handles.color = EditorSettings.gridCenterColor;
                }
                else
                {
                    Handles.color = EditorSettings.gridColor;
                }
                Handles.DrawLine(startPos, endPos);
                Handles.color = Color.white;
            }

            Handles.matrix = oldMat;
        }

        public void DrawMap(TilemapData map)
        {

            Handles.color = new Color(1, 1, 1, 0.1f);
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    if (map.IsBlock(x, y))
                    {
                        var pos = Creator.GridToWorldPosition(x, y);
                        Handles.CubeHandleCap(0, pos, Quaternion.identity, Creator.Tilemap.scale, Event.current.type);
                    }
                }
            }
        }

        void OnGridDown(Vector2Int grid)
        {

            if (selectedMap != null)
            {
                var newValue = selectedMap[Creator.GridToIndex(mouseOverGrid)];
                if (Event.current.button == 0)
                {
                    newValue = TilemapData.BLOCK;
                }
                else
                {
                    newValue = !TilemapData.BLOCK;
                }
                if (selectedMap[Creator.GridToIndex(mouseOverGrid)] != newValue)
                {
                    selectedMap[Creator.GridToIndex(mouseOverGrid)] = newValue;
                    Creator.isDirtied = true;
                    Creator.BuildTileObject();
                }

            }

        }

        public static void SelectLayer(int layer)
        {
            var creator = CreatorInstance;
            var config = creator.Tilemap;

            if (TilemapCreator.selectedLayerIndex >= 0 && TilemapCreator.selectedLayerIndex < config.layers.Length)
            {
                for (int i = 0; i < config.layers.Length; i++)
                    config.layers[i].flags &= ~TilemapLayerFlags.Hide;
            }

            TilemapCreator.selectedLayerIndex = layer;
            //if (EditorSettings.onlyShowActiveLayer)
            //{
            //    if (layer >= 0 && layer < config.layers.Length)
            //    {
            //        for (int i = 0; i < config.layers.Length; i++)
            //        {
            //            if (i != layer)
            //                config.layers[i].flags |= TilemapLayerFlags.Hide;
            //        }
            //    }
            //}
            // creator.BuildTileObject();
        }

        void DrawTilemapData(TilemapData map, Vector3 offset, bool isBlock, Color color)
        {
            var creator = Creator;
            var config = creator.Tilemap;
            int gridWidth = creator.Width, gridHeight = creator.Height;
            var oldColor = Handles.color;
            Handles.color = color;
            int index;
            Vector3 pos;
            EventType eventType = Event.current.type;
            //if (!(eventType == EventType.Repaint || eventType == EventType.Layout))
            //    eventType = EventType.Used;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    index = creator.GridToIndex(x, y);
                    pos = creator.GridToWorldPosition(x, y);
                    pos += offset;
                    if ((isBlock && map[index] == TilemapData.BLOCK) || (!isBlock && map[index] != TilemapData.BLOCK))
                    {
                        Handles.CubeHandleCap(0, pos, creator.Root.rotation, config.scale, eventType);
                    }
                }
            }
            Handles.color = oldColor;
        }


        void OnHoverGridChanged(int x, int y)
        {
            mouseOverGrid = new Vector2Int(x, y);

            Repaint();
            EditorApplication.delayCall += () =>
            {
                Repaint();
            };
        }

        public void OnSceneGUI()
        {

            var creator = Creator;
            var config = creator.Tilemap;
            if (config == null)
                return;

            int gridWidth = creator.Width, gridHeight = creator.Height;
            float lineMaxWidth = creator.Width * config.scale;
            float lineMaxHeight = creator.Height * config.scale;

            layerOffset = new Vector3();

            var selectedLayer = creator.SelectedLayer;
            if (selectedLayer != null)
            {
                layerOffset.y = selectedLayer.offsetHeight;
            }

            if (EditorSettings.showGrid)
                DrawGrid();

            if (TilemapCreator.debugDrawMap != null)
                DrawMap(TilemapCreator.debugDrawMap);

            Event evt = Event.current;
            EventType drawColorGridEventType = evt.type;
            if (drawColorGridEventType != EventType.Repaint)
                drawColorGridEventType = EventType.Used;
            int index;
            Vector3 pos;
            //var map = creator.Map;


            Handles.color = Color.white;
            var scene = SceneView.currentDrawingSceneView;
            isMouseOverGrid = IsMouseOverGrid(Event.current.mousePosition);
            if (isMouseOverGrid)
            {
                Vector2Int newGrid;
                if (ScreenPointToGrid(evt.mousePosition, out newGrid))
                {
                    if (newGrid != mouseOverGrid)
                    {
                        OnHoverGridChanged(newGrid.x, newGrid.y);
                    }
                }
            }

            if (isMouseOverGrid)
            {
                Handles.color = EditorSettings.hoverColor;
                pos = creator.GridToWorldPosition(mouseOverGrid) + layerOffset;
                Handles.CubeHandleCap(0, pos, creator.Root.rotation, config.scale, drawColorGridEventType);
                Handles.color = Color.white;
            }


            if (isMouseOverGrid /*&& evt.type == EventType.Layout*/)
            {

                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
            }

            if (TilemapCreator.selectedLayerIndex >= 0 && TilemapCreator.selectedLayerIndex < creator.Tilemap.layers.Length)
            {
                selectedMap = creator.Maps[TilemapCreator.selectedLayerIndex];
            }
            else
            {
                selectedMap = null;
                TilemapCreator.selectedLayerIndex = -1;
            }
            if (selectedMap != null)
                DrawTilemapData(selectedMap, layerOffset, false, EditorSettings.groundColor);
            if (evt.type == EventType.MouseDown)
            {
                if (isMouseOverGrid)
                {
                    if (evt.button == 0 || evt.button == 1)
                    {
                        isDown = true;

                        OnGridDown(mouseOverGrid);
                    }
                    evt.Use();
                }
            }
            if (isDown)
            {
                if (isMouseOverGrid)
                {

                }

                if (evt.type == EventType.MouseUp)
                {
                    isDown = false;
                    if (evt.button != 2)
                    {
                        evt.Use();
                    }
                }
                else if (evt.type == EventType.MouseDrag)
                {
                    if (evt.button != 2)
                    {
                        evt.Use();
                    }
                }
            }

            //if (!TilePattern.DefaultPattern.ContainsKey(patternTileType))
            //    patternTileType = TileType.Block;
            //var patterns = TilePattern.DefaultPattern[patternTileType];
            //DrawPattern(patterns, patternAngle);

            //if (evt.type == EventType.Repaint)
            //    scene.Repaint();
            if (selectedLayer != null)
            {
                if (EditorSettings.showClosedBlockNumber)
                {
                    SceneDrawClosedBlockNumber(TilemapCreator.selectedLayerIndex);
                }
            }

            if (EditorSettings.hideUnselectedLayer && TilemapCreator.selectedLayerIndex >= 0)
            {
                for (int i = 0; i < creator.Tilemap.layers.Length; i++)
                {
                    var layerRoot = creator.GetLayerRoot(i, false);
                    if (layerRoot)
                    {
                        if (i == TilemapCreator.selectedLayerIndex)
                        {
                            if (!layerRoot.gameObject.activeSelf)
                                layerRoot.gameObject.SetActive(true);
                        }
                        else
                        {
                            if (layerRoot.gameObject.activeSelf)
                                layerRoot.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < creator.Tilemap.layers.Length; i++)
                {
                    var layerRoot = creator.GetLayerRoot(i, false);
                    if (layerRoot)
                    {
                        if (!layerRoot.gameObject.activeSelf)
                            layerRoot.gameObject.SetActive(true);
                    }
                }
            }


            Handles.BeginGUI();
            {
                using (new GUILayout.VerticalScope())
                {
                    TileFlags tileFlags = TileFlags.None;
                    if (selectedMap != null)
                        tileFlags = selectedMap.GetTileFlags(mouseOverGrid.x, mouseOverGrid.y);
                    //GUILayout.Label("tile type:" + tileFlags.ToTileType() + ", " + tileFlags);
                    GUILayout.Label(mouseOverGrid + ", " + tileFlags.ToTileType() + ", " + tileFlags);
                }
            }
            Handles.EndGUI();


        }

        void SceneDrawClosedBlockNumber(int layer)
        {
            var creator = Creator;
            Vector3 layerOffset = creator.GetLayerOffset(layer);
            int nextId = 1;
            Vector3 pos;
            Vector2 screenPos;
            GUIStyle labelStyle = new GUIStyle("label");
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = EditorSettings.closedBlockNumberColor;
            Handles.BeginGUI();
            {
                foreach (var blocks in creator.GetMap(layer).EnumerateClosedBlocks())
                {
                    GUIContent label = new GUIContent(nextId.ToString());
                    var labelSize = labelStyle.CalcSize(label);

                    foreach (var block in blocks)
                    {
                        pos = creator.GridToWorldPosition(block) + layerOffset;
                        screenPos = HandleUtility.WorldToGUIPoint(pos);
                        screenPos.x -= labelSize.x * 0.5f;
                        screenPos.y -= labelSize.y * 0.5f;
                        GUI.Label(new Rect(screenPos.x, screenPos.y, labelSize.x, labelSize.y), label, labelStyle);
                    }
                    nextId++;
                }
            }
            Handles.EndGUI();
        }




        public float patternAngle;
        public TileType patternTileType = TileType.Block;
        void DrawPattern(TilePattern pattern, float rotation)
        {
            TilePatternGrid patternGrid;
            int x, y;

            for (int i = 0; i < pattern.Length; i++)
            {
                patternGrid = pattern[i];
                x = patternGrid.X;
                y = patternGrid.Y;
                x += mouseOverGrid.x;
                y += mouseOverGrid.y;
                if (patternGrid.IsBlock)
                {
                    Handles.color = new Color(0, 1, 0, 0.5f);
                }
                else
                {
                    Handles.color = new Color(1, 0, 0, 0.5f);
                }
                Handles.CubeHandleCap(0, Creator.GridToWorldPosition(x, y), Quaternion.identity, 1, Event.current.type);
            }
            Handles.color = Color.white;
        }



        public Vector2Int mouseOverGrid;
        public bool isMouseOverGrid;
        private bool isDown;
        TileFlags selectedSide;
        static Ray ray;
        Editor cachedCreatorEditor;

        [MenuItem("Assets/Create/Tilemap/Tilemap")]
        static void CreateTilemap()
        {
            string path = "Assets/Tilemap.asset";
            if (Selection.instanceIDs.Length > 0)
            {
                string path2 = AssetDatabase.GetAssetPath(Selection.instanceIDs[0]);
                if (!string.IsNullOrEmpty(path2))
                {
                    if (File.Exists(path2))
                    {
                        path2 = Path.GetDirectoryName(path2);
                    }
                    path = path2 + "/Tilemap.asset";
                }
            }
            CreateTilemap(path);
        }

        static Tilemap CreateTilemap(string path)
        {
            var asset = ScriptableObject.CreateInstance<Tilemap>();
            asset.width = 10;
            asset.height = 10;
            asset.tiles = new TileGroup[] { new TileGroup() };
            asset.layers = new TilemapLayer[] { new TilemapLayer() { enabled = true } };

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
            var config = Creator.Tilemap;
            if (config == null)
            {
                if (GUILayout.Button("Create Tilemap", GUILayout.ExpandWidth(true)))
                {
                    var scene = EditorSceneManager.GetActiveScene();
                    string dir = null;
                    if (!string.IsNullOrEmpty(scene.path))
                    {
                        dir = Path.GetDirectoryName(scene.path);
                    }
                    if (string.IsNullOrEmpty(dir))
                        dir = "Assets";

                    string path = EditorUtility.SaveFilePanel("Tilemap", dir, "Tilemap.asset", "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        if (Path.IsPathRooted(path))
                        {
                            string baseDir = Path.GetFullPath(".");
                            path = path.Substring(baseDir.Length);
                            if (path.Length > 0 && (path[0] == '/' || path[0] == '\\'))
                                path = path.Substring(1);
                        }
                        var asset = CreateTilemap(path);
                        //EditorUtility.SetDirty(asset);
                        //asset = AssetDatabase.LoadAssetAtPath<TilemapConfigAsset>(path);
                        Creator.Tilemap = asset;
                    }
                }
                return;
            }
            //if (GUILayout.Button("Build"))
            //{
            //    Creator.Map.CopyTemplate();
            //}
            //patternAngle = EditorGUILayout.FloatField("patten angle", patternAngle);
            //patternTileType = (TileType)EditorGUILayout.EnumPopup("patten ", patternTileType);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Build"))
                {
                    Creator.Build();
                    EditorUtility.SetDirty(Creator.gameObject);
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Build");
                if (GUILayout.Button("Tiles"))
                {
                    Creator.BuildTileObject();
                }

                //selectedSide = (TileSideType)EditorGUILayout.EnumPopup(selectedSide);
                //if (GUILayout.Button("bsp"))
                //{
                //    Vector3 startPos, endPos;
                //    int mapWidth = config.width / 2;
                //    int mapHeight = config.height / 2;
                //    var tmp = new BSPDungeonAlgorithm().Generate(config, mapWidth, mapHeight, out startPos, out endPos);

                //    for (int i = 0; i < mapHeight; i++)
                //    { 
                //        for (int j = 0; j < mapWidth; j++)
                //        {
                //            Creator.Map[j, i] = tmp[j, i];
                //        }
                //    }

                //    Creator.Map.CopyTemplate();
                //}
                if (GUILayout.Button("Decorates"))
                {
                    Creator.BuildDecorateObject();

                }

            }


            if (GUILayout.Button("Clear"))
            {
                Creator.Clear();
            }

            using (new GUILayout.HorizontalScope())
            {
                var tilemap = Creator.Tilemap;

                var dataSettings = tilemap.data;
                var dataProvider = dataSettings.provider;
                if (dataProvider != null && dataProvider.GetType() == typeof(TilemapDataProvider))
                    dataProvider = null;

                if (dataProvider == null)
                    GUI.enabled = false;

                if (GUILayout.Button("Load"))
                {

                    string path = EditorUtility.OpenFilePanel("Load Tilemap Data", "Assets", dataProvider.FileExtensionName);
                    if (!string.IsNullOrEmpty(path))
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            dataProvider.Read(Creator, dataSettings, fs);
                            Creator.BuildTileObject();
                            Creator.BuildDecorateObject();
                            EditorUtility.SetDirty(Creator.gameObject);
                        }
                    }
                }


                if (GUILayout.Button("SaveAs"))
                {
                    if (dataProvider != null)
                    {
                        string path = EditorUtility.SaveFilePanel("Save Tilemap Data", "Assets", "tilemap", dataProvider.FileExtensionName);
                        if (!string.IsNullOrEmpty(path))
                        {
                            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                            {
                                dataProvider.Write(Creator, dataSettings, fs);
                            }
                        }
                    }
                }

                GUI.enabled = true;

            }

            if (Creator.Tilemap)
            {
                Editor.CreateCachedEditor(Creator.Tilemap, null, ref cachedCreatorEditor);
                cachedCreatorEditor.OnInspectorGUI();
            }

        }



        static GameObject InstantiateGameObject(TilemapInstantiateData instantiateData)
        {
            GameObject go;

            if (PrefabUtility.GetPrefabAssetType(instantiateData.prefab) != PrefabAssetType.NotAPrefab)
            {
                go = PrefabUtility.InstantiatePrefab(instantiateData.prefab) as GameObject;
            }
            else
            {
                go = Instantiate(instantiateData.prefab) as GameObject;
            }
            go.transform.parent = instantiateData.parent;
            go.transform.localPosition = instantiateData.position;
            go.transform.localEulerAngles = instantiateData.rotation;
            go.transform.localScale = Vector3.Scale(go.transform.localScale, instantiateData.scale);
            return go;
        }
        public void DrawBlock()
        {

        }


        public bool IsMouseOverGrid(Vector2 mousePos)
        {
            Vector2Int grid;
            if (ScreenPointToGrid(mousePos, out grid))
            {
                var creator = Creator;
                if (!creator.IsValidGrid(grid))
                    return false;
            }

            return true;
        }


        public bool ScreenPointToGrid(Vector2 mousePos, out Vector2Int grid)
        {
            var creator = Creator;
            var evt = Event.current;
            var ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            Plane plane = new Plane(creator.transform.up, creator.transform.position + layerOffset);
            float dist;
            if (plane.Raycast(ray, out dist))
            {
                grid = creator.WorldPositionToGrid(ray.GetPoint(dist));
                return true;
            }
            grid = new Vector2Int();
            return false;
        }

        [MenuItem("GameObject/3D Object/Tilemap")]
        public static void CreateTilemapCreator()
        {
            GameObject go = new GameObject("Tilemap");
            var creator = go.AddComponent<TilemapCreator>();
        }


        #region TilemapEditorSettings

        const string TilemapEditorSettingsPath = "Assets/Editor/Settings/TilemapEditorSettings.asset";
        static TilemapEditorSettings tilemapEditorSettings;

        public static TilemapEditorSettings EditorSettings
        {
            get
            {
                if (!tilemapEditorSettings)
                {
                    tilemapEditorSettings = GetOrCreateTilemapEditorSettings();
                }
                return tilemapEditorSettings;
            }
        }

        [SettingsProvider]
        static SettingsProvider CreateTilemapEditorSettings()
        {
            var provider = new SettingsProvider("Tilemap Editor", SettingsScope.User)
            {
                label = "Tilemap Editor",
                guiHandler = OnGUITilemapEditorSettings,
                keywords = new HashSet<string>(new[] { "Tile", "Tilemap" })
            };

            return provider;
        }

        static void OnGUITilemapEditorSettings(string searchContext)
        {
            var settings = GetSerializedTilemapEditorSettings();
            using (var checker = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField("Grid");
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(settings.FindProperty("showGrid"), new GUIContent("Show Grid"));
                    EditorGUILayout.PropertyField(settings.FindProperty("gridColor"), new GUIContent("Grid Color"));
                    EditorGUILayout.PropertyField(settings.FindProperty("gridCenterColor"), new GUIContent("Grid Center Color"));
                    EditorGUILayout.PropertyField(settings.FindProperty("hoverColor"), new GUIContent("Hover Color"));
                }
                EditorGUI.indentLevel--;


                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Selected Layer");
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(settings.FindProperty("groundColor"), new GUIContent("Ground Color"));
                    EditorGUILayout.PropertyField(settings.FindProperty("hideUnselectedLayer"), new GUIContent("Hide Unselected Layer"));
                    EditorGUILayout.PropertyField(settings.FindProperty("showClosedBlockNumber"), new GUIContent("Closed Block Number"));
                    EditorGUILayout.PropertyField(settings.FindProperty("closedBlockNumberColor"), new GUIContent("Closed Block Number"));
                }
                EditorGUI.indentLevel--;

                if (checker.changed)
                {
                    settings.ApplyModifiedProperties();
                }
            }
        }


        static TilemapEditorSettings GetOrCreateTilemapEditorSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TilemapEditorSettings>(TilemapEditorSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<TilemapEditorSettings>();
          

                if (!Directory.Exists(Path.GetDirectoryName(TilemapEditorSettingsPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(TilemapEditorSettingsPath));

                AssetDatabase.CreateAsset(settings, TilemapEditorSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
        static SerializedObject GetSerializedTilemapEditorSettings()
        {
            return new SerializedObject(GetOrCreateTilemapEditorSettings());
        }


        #endregion

    }


}