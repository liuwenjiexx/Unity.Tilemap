using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;


namespace Unity.Tilemaps
{

    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {

        private Dictionary<TileType, TileItemSetting> itemSettings;

        static GameObject[] selectedPrefabs;
        [MenuItem("Assets/Tilemap/Select Prefabs")]
        static void SelectPrefabs()
        {
            selectedPrefabs = Selection.gameObjects.OrderBy(o=>o.name).ToArray();
        }

        #region Preview


        private PreviewRenderUtility previewRender;
        private Transform previewRoot;
        private bool isDraging;
        private Vector2 dragStartPos;
        private float distance;
        private float size;
        private float height;
        private float dragSpeed = 1f;

        static int selectedPreviewIndex;
        static bool showGrid;
        static int PreviewCount = 3;

        #endregion


        [Serializable]
        public class TilePreviewSettings
        {
            public Color ambientColor = Color.white;
            public float lightIntensity = 0.5f;

            static string PrefsKey = typeof(TilePreviewSettings).FullName;
            static TilePreviewSettings instance;

            public static TilePreviewSettings Instance
            {
                get
                {
                    if (instance == null)
                    {
                        try
                        {
                            string json = EditorPrefs.GetString(PrefsKey);
                            if (!string.IsNullOrEmpty(json))
                            {
                                instance = JsonUtility.FromJson<TilePreviewSettings>(json);
                            }
                        }
                        catch { }
                        if (instance == null)
                        {
                            instance = new TilePreviewSettings();
                            instance.Save();
                        }

                    }
                    return instance;
                }
            }

            public void Save()
            {
                EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(this));
            }
        }



        private class TileItemSetting
        {
            public Texture2D icon;
            public int selectedIndex;
        }


        Tile Asset
        {
            get { return target as Tile; }
        }


        [MenuItem("Assets/Create/Tilemap/Tile", true, priority = 0)]
        static bool CreateTileAssetValidate()
        {
            return true;
        }

        [MenuItem("Assets/Create/Tilemap/Tile", false, priority = 0)]
        static void CreateTileAsset()
        {
            string assetDir = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            Tile asset = null;


            if (Directory.Exists(assetDir))
            {
                asset = ScriptableObject.CreateInstance<Tile>();
                asset.edge = new TileItem() { tileType = TileType.Edge };
                asset.outerCorner = new TileItem() { tileType = TileType.OuterCorner };
                asset.innerCorner = new TileItem() { tileType = TileType.InnerCorner };
                asset.block = new TileItem() { tileType = TileType.Block };
                asset.ground = new TileItem() { tileType = TileType.Ground };

            }
            //else
            //{
            //    asset = ScriptableObject.CreateInstance<Tile>();
            //    assetDir = null;
            //    if (Selection.gameObjects.Length > 0)
            //    {
            //        var gos = Selection.gameObjects.Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o))).
            //            OrderBy(o => o.name).ToArray();

            //        if (gos.Length > 0)
            //        {
            //            //Tile tile = new Tile();
            //            TileItem[] items = new TileItem[Mathf.Min(gos.Length, 5)];
            //            if (gos.Length > 0)
            //                items[0] = new TileItem() { tileType = TileType.Edge };
            //            if (gos.Length > 1)
            //                items[1] = new TileItem() { tileType = TileType.OuterCorner };
            //            if (gos.Length > 2)
            //                items[2] = new TileItem() { tileType = TileType.InnerCorner };
            //            if (gos.Length > 3)
            //                items[3] = new TileItem() { tileType = TileType.Block };
            //            if (gos.Length > 4)
            //                items[4] = new TileItem() { tileType = TileType.Ground };
            //            //tile.items = items;
            //            //asset.tile = tile;
            //            asset.items = items;
            //            assetDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(gos[0]));
            //        }
            //    }

            //    if (string.IsNullOrEmpty(assetDir))
            //        assetDir = "Assets";

            //}

            if (!asset)
                return;

            int n = 0;
            string assetPath = null;
            while (true)
            {
                if (n == 0)
                {
                    assetPath = Path.Combine(assetDir, "Tile.asset");
                }
                else
                {
                    assetPath = Path.Combine(assetDir, string.Format("Tile ({0}).asset", n));
                }
                if (!File.Exists(assetPath))
                    break;
                n++;
            }
            AssetDatabase.CreateAsset(asset, assetPath);
        }



        void Initialize()
        {

            if (itemSettings == null)
            {
                itemSettings = new Dictionary<TileType, TileItemSetting>();

                string resDir = Path.Combine(TilemapCreatorEditor.PackageDir, "Res");
                foreach (TileType tileType in Enum.GetValues(typeof(TileType)))
                {
                    itemSettings[tileType] = new TileItemSetting()
                    {
                        icon = AssetDatabase.LoadAssetAtPath<Texture2D>(resDir + string.Format("/Icons/{0}.png", tileType)),
                    };
                }
            }


        }

        IEnumerable<TileItem> OrderBy(IEnumerable<TileItem> tileItems)
        {
            return tileItems.OrderBy(o => o.usePattern)
                  .OrderBy(o =>
             {
                 switch (o.tileType)
                 {
                     case TileType.Edge:
                         return 0;
                     case TileType.OuterCorner:
                         return 1;
                     case TileType.InnerCorner:
                         return 2;
                     case TileType.Block:
                         return 3;
                     case TileType.Ground:
                         return 4;
                 }
                 return 5;
             });
        }

        IEnumerable<TileItem> GetItems()
        {
            if (Asset.edge != null)
                yield return Asset.edge;
            if (Asset.block != null)
                yield return Asset.block;
            if (Asset.ground != null)
                yield return Asset.ground;
            if (Asset.innerCorner != null)
                yield return Asset.innerCorner;
            if (Asset.outerCorner != null)
                yield return Asset.outerCorner;
        }

        public override void OnInspectorGUI()
        {
            var asset = Asset;
            var tile = asset;
            Initialize();

            float space = 2;
            var items = OrderBy(GetItems()).ToArray();
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item.tileType == 0)
                    continue;
                if (i > 0)
                    GUILayout.Space(space);
                DrawTileItems(tile, item);

            }



            if (GUILayout.Button("prefab > Items (1)"))
            {
                for (int i = 0; i < tile.items.Length; i++)
                {
                    var item = tile.items[i];
                    if (item.prefab && (item.items == null || item.items.Length == 0))
                    {
                        item.items = new TilePrefab[] {
                                            new TilePrefab() {
                                                prefab= item.prefab,
                                                 weight=1f,
                                            }
                                        };
                    }
                }
            }

            if (GUILayout.Button("items > edge, block"))
            {
                for (int i = 0; i < tile.items.Length; i++)
                {
                    var item = tile.items[i];
                    switch (item.tileType)
                    {
                        case TileType.Edge:
                            tile.edge = item;
                            break;
                        case TileType.Block:
                            tile.block = item;
                            break;
                        case TileType.Ground:
                            tile.ground = item;
                            break;
                        case TileType.InnerCorner:
                            tile.innerCorner = item;
                            break;
                        case TileType.OuterCorner:
                            tile.outerCorner = item;
                            break;
                    }
                }
                tile.items = new TileItem[0];
            }



            using (new GUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Preview");
                    GUILayout.FlexibleSpace();
                }
                using (var checker = new EditorGUI.ChangeCheckScope())
                {
                    TilePreviewSettings.Instance.ambientColor = EditorGUILayout.ColorField("Ambient Color", TilePreviewSettings.Instance.ambientColor);
                    TilePreviewSettings.Instance.lightIntensity = EditorGUILayout.Slider("Light Intensity", TilePreviewSettings.Instance.lightIntensity, 0f, 1f);
                    if (checker.changed)
                    {
                        TilePreviewSettings.Instance.Save();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                SetAssetDirty();
            }


        }


        void DrawTileItems(Tile tile, TileItem tileItem)
        {
            var asset = Asset;
            //if (items == null || items.Length == 0)
            //{
            //    items = new TileItem[] { new TileItem() { tileType = tileType } };
            //    GUI.changed = true;
            //}
            TileType tileType = tileItem.tileType;
            int displaySIze = 36;
            Event evt = Event.current;
            using (new GUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(tileType.ToString());
                    GUILayout.FlexibleSpace();
                }

                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        Texture2D icon = null;

                        icon = itemSettings[tileType].icon;
                        Rect rect = GUILayoutUtility.GetRect(displaySIze, displaySIze, displaySIze, displaySIze, GUILayout.ExpandWidth(false));

                        //if (evt.type == EventType.Repaint)
                        //    GUI.DrawTexture(rect, icon);

                        DrawPatternPreview(rect, tileItem.GetPattern());

                        if (evt.type == EventType.MouseDown && evt.clickCount == 2 && rect.Contains(evt.mousePosition))
                        {
                            patternItemEdit = tileItem;
                            patternEdit = patternItemEdit.GetPattern().Clone();
                            tileTypeEdit = patternItemEdit.tileType;
                        }

                        if (rect.Contains(evt.mousePosition))
                        {
                            if (evt.type == EventType.ContextClick)
                            {
                                GenericMenu menu = new GenericMenu();
                                if (selectedPrefabs != null && selectedPrefabs.Length > 0)
                                {
                                    menu.AddItem(new GUIContent("Add Selected Prefabs"), false, () =>
                                     {
                                         if (tileItem.items == null)
                                             tileItem.items = new TilePrefab[0];
                                         for (int j = 0; j < selectedPrefabs.Length; j++)
                                         {
                                             if (tileItem.ContainsPrefab(selectedPrefabs[j]))
                                                 continue;
                                             var _prefabItem = new TilePrefab() { prefab = selectedPrefabs[j], weight = 1f };
                                             tileItem.items = tileItem.items.InsertArrayElementAtIndex(_prefabItem);
                                         }
                                         selectedPrefabs = null;
                                         SetAssetDirty();
                                     });
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Add Selected Prefabs"));
                                }
                                menu.ShowAsContext();
                            }
                        }


                        if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
                        {
                            if (rect.Contains(evt.mousePosition))
                            {
                                GameObject[] gos = DragAndDrop.objectReferences.Select(o => o as GameObject).Where(o => o).ToArray();

                                if (gos.Length > 0)
                                {
                                    if (evt.type == EventType.DragPerform)
                                    {
                                        gos = gos.OrderBy(o => o.name).ToArray();

                                        if (tileItem.items == null)
                                            tileItem.items = new TilePrefab[0];
                                        for (int j = 0; j < gos.Length; j++)
                                        {
                                            var _prefabItem = new TilePrefab() { prefab = gos[j], weight = 1f };
                                            tileItem.items = tileItem.items.InsertArrayElementAtIndex(_prefabItem);
                                        }

                                        GUI.changed = true;

                                        DragAndDrop.AcceptDrag();
                                        evt.Use();
                                    }
                                    else
                                    {
                                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                    }
                                }
                            }
                        }


                    }
                    using (new GUILayout.VerticalScope())
                    {

                        GUILayout.Space(0);


                        TilePrefab firstPrefab = new TilePrefab();

                        if (tileItem.items != null)
                            firstPrefab = tileItem.items.FirstOrDefault();

                        //if (i > 0)
                        //{

                        //    GUILayout.Space(3);
                        //    Rect lineRect = GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true), GUILayout.Height(1));
                        //    lineRect.GUIFillRect(new Color(0, 0, 0, 0.5f));
                        //    GUILayout.Space(3);
                        //}

                        using (new GUILayout.HorizontalScope())
                        {

                            using (new GUILayout.VerticalScope())
                            {

                                Rect rect = GUILayoutUtility.GetRect(displaySIze, displaySIze, displaySIze, displaySIze, GUILayout.ExpandWidth(false));

                                GUIGameObjectPreviewBox(rect, tileItem, 0);
                                GUILayout.Label(firstPrefab.prefab ? firstPrefab.prefab.name : "missing", NameStyle);
                            }

                            using (new GUILayout.VerticalScope())
                            {

                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Rotation", GUILayout.Width(labelWidth));
                                    tileItem.rotation = EditorGUILayout.Vector3Field(GUIContent.none, tileItem.rotation);
                                }
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label("Offset", GUILayout.Width(labelWidth), GUILayout.MaxWidth(labelWidth));
                                    tileItem.offset = EditorGUILayout.Vector3Field(GUIContent.none, tileItem.offset);
                                }
                                if (tileItem.items.Length > 0)
                                {
                                    if (!tileItem.IsEmpty)
                                        firstPrefab = tileItem.items.FirstOrDefault();

                                    firstPrefab = GUITilePrefabProperty(firstPrefab);
                                    if (!tileItem.IsEmpty)
                                        tileItem.items[0] = firstPrefab;
                                }
                            }
                        }



                        for (int j = 1; j < tileItem.items.Length; j++)
                        {
                            var prefabItem = tileItem.items[j];

                            using (new GUILayout.HorizontalScope())
                            {
                                using (new GUILayout.VerticalScope())
                                {
                                    Rect rect = GUILayoutUtility.GetRect(displaySIze, displaySIze, displaySIze, displaySIze, GUILayout.ExpandWidth(false));

                                    GUIGameObjectPreviewBox(rect, tileItem, j);
                                    GUILayout.Label(prefabItem.prefab ? prefabItem.prefab.name : "missing", NameStyle);
                                }
                                prefabItem = tileItem.items[j];
                                using (new GUILayout.VerticalScope())
                                {
                                    prefabItem = GUITilePrefabProperty(prefabItem);
                                }
                                tileItem.items[j] = prefabItem;
                            }
                            GUILayout.Space(1);


                        }

                        GUILayout.Space(1);

                    }

                }
            }

        }
        GUIStyle nameStyle;
        GUIStyle NameStyle
        {
            get
            {
                if (nameStyle == null)
                {
                    nameStyle = new GUIStyle("label");
                    nameStyle.fontSize -= 6;
                }
                return nameStyle;
            }
        }

        float labelWidth = 60;
        TilePrefab GUITilePrefabProperty(TilePrefab tilePrefab)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Weight", GUILayout.Width(labelWidth), GUILayout.MaxWidth(labelWidth));
                tilePrefab.weight = EditorGUILayout.FloatField(GUIContent.none, tilePrefab.weight);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(new GUIContent("Rotation", "use Ground Random Rotatable"), GUILayout.Width(labelWidth), GUILayout.MaxWidth(labelWidth));
                tilePrefab.rotation = EditorGUILayout.Toggle(GUIContent.none, tilePrefab.rotation);
            }

            return tilePrefab;
        }

        void GUIGameObjectPreviewBox(Rect rect, TileItem tileItem, int index)
        {
            Texture2D preview = null;
            Event evt = Event.current;
            TilePrefab prefabItem = new TilePrefab();
            TileType tileType = tileItem.tileType;
            if (tileItem.items == null)
                tileItem.items = new TilePrefab[0];

            if (index < tileItem.items.Length)
            {
                prefabItem = tileItem.items[index];
                if (prefabItem.prefab)
                {
                    preview = AssetPreview.GetAssetPreview(prefabItem.prefab);
                }
                else
                {

                }
            }

            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (rect.Contains(evt.mousePosition))
                {
                    GameObject[] gos = DragAndDrop.objectReferences.Select(o => o as GameObject).Where(o => o).ToArray();

                    if (gos.Length > 0)
                    {
                        if (evt.type == EventType.DragPerform)
                        {

                            if (gos.Length > 1)
                            {
                                UpdatePrefab(gos);
                            }
                            else
                            {

                                if (index >= tileItem.items.Length)
                                {
                                    prefabItem = new TilePrefab() { prefab = gos[0], weight = 1f };
                                    tileItem.items = new TilePrefab[] { prefabItem };
                                }
                                else
                                {
                                    prefabItem.prefab = gos[0];
                                    tileItem.items[index] = prefabItem;
                                }
                            }
                            GUI.changed = true;
                            DragAndDrop.AcceptDrag();

                            evt.Use();
                        }
                        else
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        }

                    }
                }
            }

            GUIStyle boxStyle = new GUIStyle("box");
            boxStyle.padding = new RectOffset();

            if (GUI.Button(rect, new GUIContent(preview, prefabItem.prefab ? prefabItem.prefab.name : ""), boxStyle))
            {
                if (Event.current.button == 1)
                {

                    GenericMenu menu = new GenericMenu();
                    //menu.AddItem(new GUIContent("None"), false, (o) =>
                    //{
                    //    int index = (int)o;
                    //    items[index].prefab = null;
                    //    SetAssetDirty();
                    //}, i); 

                    //menu.AddItem(new GUIContent("Edit"), false, () =>
                    //{

                    //});


                    if (index < tileItem.items.Length)
                    {
                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            tileItem.items = tileItem.items.DeleteArrayElementAtIndex(index);
                            SetAssetDirty();
                        });
                    }

                    menu.ShowAsContext();
                }
                else
                {
                    if (prefabItem.prefab)
                    {
                        EditorGUIUtility.PingObject(prefabItem.prefab);
                    }


                    itemSettings[tileType].selectedIndex = index;
                }
            }



            if (index < tileItem.items.Length)
            {
                if (itemSettings[tileType].selectedIndex == index)
                {
                    new RectOffset(1, 1, 1, 1).Add(rect)
                           .GUIDrawBorder(1, new Color(0, 0, 1f, 0.5f));
                }

                tileItem.items[index] = prefabItem;

            }
        }

        TileType ParseTileType(string name)
        {
            foreach (TileType tileType in Enum.GetValues(typeof(TileType)))
            {
                if (name.IndexOf(tileType.ToString()) >= 0)
                    return tileType;
            }
            return 0;
        }

        public void UpdatePrefab(GameObject[] gos)
        {
            foreach (var go in gos)
            {
                TileType tileType = ParseTileType(go.name);
                if (tileType == 0)
                    continue;
                var tileItem = GetTileItem(Asset, tileType);
                if (tileItem == null)
                    continue;
                TilePrefab tilePrefab;
                if (tileItem.items == null || tileItem.items.Length == 0)
                    tilePrefab = new TilePrefab() { prefab = go, weight = 1f };
                else
                    tilePrefab = tileItem.items[0];
                tilePrefab.prefab = go;

                tileItem.items[0] = tilePrefab;
            }
            SetAssetDirty();
        }


        void SetAssetDirty()
        {
            if (!Application.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                serializedObject.UpdateIfRequiredOrScript();
                SetPreviewDiry();
            }
        }


        private void InitPreview()
        {
            if (previewRender == null)
            {
                previewRender = new PreviewRenderUtility(false);

                previewRender.cameraFieldOfView = 60f;
                previewRender.camera.nearClipPlane = 0.1f;
                previewRender.camera.farClipPlane = 100;
                previewRender.camera.clearFlags = CameraClearFlags.Color;
            }

            if (!previewRoot)
            {
                previewRoot = new GameObject("TilePreview").transform;
                previewRender.AddSingleGO(previewRoot.gameObject);
                previewRender.camera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
                CreatePreviewObjects(previewRoot);
                distance = size;
            }

        }
        Tile previewTile;

        void CreatePreviewObjects(Transform parent)
        {
            var asset = Asset;
            var tile = asset;
            Initialize();

            float lineSize = 0.005f;
            float lineLength = 1;
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.parent = parent;
            line.transform.localPosition = new Vector3(lineLength * 0.5f, 0, -lineSize * 0.5f);
            line.transform.localEulerAngles = Vector3.zero;
            line.transform.localScale = new Vector3(lineLength, lineSize, lineSize);
            line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.parent = parent;
            line.transform.localPosition = new Vector3(-lineSize * 0.5f, 0, lineLength * 0.5f);
            line.transform.localEulerAngles = Vector3.zero;
            line.transform.localScale = new Vector3(lineSize, lineSize, lineLength);


            if (!previewTile)
            {
                previewTile = ScriptableObject.CreateInstance<Tile>();
            }

            GetTile(previewTile, TileType.Edge, itemSettings[TileType.Edge].selectedIndex);
            GetTile(previewTile, TileType.OuterCorner, itemSettings[TileType.OuterCorner].selectedIndex);
            GetTile(previewTile, TileType.InnerCorner, itemSettings[TileType.InnerCorner].selectedIndex);
            GetTile(previewTile, TileType.Block, itemSettings[TileType.Block].selectedIndex);
            GetTile(previewTile, TileType.Ground, itemSettings[TileType.Ground].selectedIndex);

            TilemapData map;

            switch (selectedPreviewIndex)
            {
                case 1:
                    map = new TilemapData(4, 4);
                    map.Fill(1, 1, 2, 2, TilemapData.BLOCK);
                    break;
                case 2:
                    map = new TilemapData(4, 4);
                    break;
                default:
                    map = new TilemapData(6, 6);
                    map.Fill(1, 1, 4, 4, TilemapData.BLOCK);
                    map.SetBlock(4, 1, false);
                    break;
            }

            TilemapLayer layer = new TilemapLayer()
            {
                enabled = true,
            };
            TileGroup[] tileGroups = new TileGroup[]
            {
                new TileGroup(){ tile= previewTile}
            };

            new DefaultTileAlgorithm().Generate(null, null, map, parent, layer, tileGroups, 1f);
            Vector3 center = new Vector3(map.Width * 0.5f, 0f, map.Height * 0.5f);
            foreach (Transform child in parent)
            {
                child.position -= center;

            }

            size = Mathf.Max(map.Width, map.Height);

        }


        public TileItem GetTileItem(Tile tile, TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Edge:
                    return tile.edge;
                case TileType.Block:
                    return tile.block;
                case TileType.Ground:
                    return tile.ground;
                case TileType.InnerCorner:
                    return tile.innerCorner;
                case TileType.OuterCorner:
                    return tile.outerCorner;
            }
            return null;
        }

        public void SetTileItem(Tile tile, TileType tileType, TileItem value)
        {
            switch (tileType)
            {
                case TileType.Edge:
                    tile.edge = value;
                    break;
                case TileType.Block:
                    tile.block = value;
                    break;
                case TileType.Ground:
                    tile.ground = value;
                    break;
                case TileType.InnerCorner:
                    tile.innerCorner = value;
                    break;
                case TileType.OuterCorner:
                    tile.outerCorner = value;
                    break;
            }
        }

        void GetTile(Tile tile, TileType tileType, int index)
        {
            var item = GetTileItem(Asset, tileType);
            if (item == null || item.IsEmpty)
                return;
            TileItem newItem = new TileItem()
            {
                offset = item.offset,
                pattern = item.pattern,
                rotation = item.rotation,
                tileType = item.tileType,
                usePattern = item.usePattern,
                items = item.items,
            };
            newItem.items = new TilePrefab[] { item.items[index] };
            SetTileItem(tile, tileType, newItem);
        }

        private void OnDisable()
        {
            if (previewRender != null)
            {
                previewRender.Cleanup();
                previewRender = null;
            }

            if (previewRoot)
            {
                DestroyImmediate(previewRoot.gameObject);
            }
            if (previewTile)
            {
                UnityEngine.Object.DestroyImmediate(previewTile);
                previewTile = null;
            }
        }
        private void DestroyPreviewObjects()
        {

            if (previewRoot)
            {
                while (previewRoot.childCount > 0)
                    DestroyImmediate(previewRoot.GetChild(0).gameObject);
            }
        }

        void SetPreviewDiry()
        {
            DestroyPreviewObjects();
            InitPreview();
            CreatePreviewObjects(previewRoot);
            Repaint();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }
        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Tile Preview");
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            base.OnInteractivePreviewGUI(r, background);
        }
        int gridSize = 5;

        Vector2 GridToScreenPos(Vector2 center, int x, int y)
        {
            Vector2 pos = new Vector2(x, y) * drawGridSize;
            return pos;
        }
        Vector2Int ScreenPosToGrid(Vector2 center, Vector2 pos)
        {

            Vector2Int grid = new Vector2Int((int)(pos.x / drawGridSize), (int)(pos.y / drawGridSize));
            if (pos.x < 0)
                grid.x -= 1;
            if (pos.y < 0)
                grid.y -= 1;
            return grid;
        }

        static float drawGridSize = 25;

        public static TileItem patternItemEdit;
        static TilePattern patternEdit;
        static TileType tileTypeEdit;
        static Color BlockColor = Color.blue;
        static Color IncludeColor = new Color(0.7f, 0.7f, 0.7f, 1);
        static Color ExcludeColor = Color.red;
        static void DrawPattern(TilePattern pattern, Vector2 cellSize)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                var item = pattern[i];
                Color fillColor = Color.white;

                switch (item.Type)
                {
                    case TilePatternType.Block:
                        fillColor = BlockColor;
                        break;
                    case TilePatternType.Include:
                        fillColor = IncludeColor;
                        break;
                    case TilePatternType.Exclude:
                        fillColor = ExcludeColor;
                        break;
                }
                fillColor.GUIFillGrid(cellSize, item.X, item.Y);
            }
        }
        public void DrawPatternPreview(Rect rect, TilePattern pattern)
        {
            int minX = 0, maxX = 0, minY = 0, maxY = 0;
            for (int i = 0; i < pattern.Length; i++)
            {
                var item = pattern[i];
                if (item.X < minX)
                    minX = item.X;
                if (item.X > maxX)
                    maxX = item.X;
                if (item.Y < minY)
                    minY = item.Y;
                if (item.Y > maxY)
                    maxY = item.Y;
            }
            int gridSize;
            int gridSizeX, gridSizeY;
            gridSizeX = Mathf.Abs(minX) + maxX + 1;

            gridSizeY = Mathf.Abs(minY) + Mathf.Abs(maxY) + 1;
            gridSize = Mathf.Max(gridSizeX, gridSizeY);
            gridSize = Mathf.Max(gridSize, 3);

            float cellSize;
            float viewSize = Mathf.Min(rect.width, rect.height);
            cellSize = viewSize / gridSize;

            Vector2Int minGrid = new Vector2Int(minX, minY);
            if (gridSizeX < gridSize)
                minGrid.x -= (int)((gridSize - gridSizeX) * 0.5f);
            if (gridSizeY < gridSize)
                minGrid.y -= (int)((gridSize - gridSizeY) * 0.5f);

            Vector2 gridOffset = rect.center + new Vector2(minGrid.x * cellSize - cellSize * 0.5f, minGrid.y * cellSize - cellSize * 0.5f);
            gridOffset.x = (int)gridOffset.x;
            gridOffset.y = (int)gridOffset.y;
            Vector2 offset;
            offset.x = gridOffset.x - rect.x;
            offset.y = gridOffset.y - rect.y;
            offset = -offset;



            var oldMat = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(rect.center + new Vector2((int)(-cellSize * 0.5f), (int)(cellSize * 0.5f)), Quaternion.identity, new Vector3(1, -1, 1));
            DrawPattern(pattern, Vector2.one * cellSize);

            GUI.matrix = Matrix4x4.TRS(gridOffset + offset, Quaternion.identity, Vector3.one);
            gridColor.GUIDrawGrid(new Vector2Int(gridSize, gridSize), Vector2.one * cellSize, 1f);

            GUI.matrix = oldMat;


        }
        static Color gridColor = new Color(0, 0, 0, 0.3f);

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {

            InitPreview();
            var asset = Asset;


            if (patternEdit != null)
            {

                Vector2 pos;

                if (patternEdit == null)
                {
                    patternEdit = new TilePattern();

                }
                var centerIndex = patternEdit.FindIndex(0, 0);
                if (centerIndex < 0)
                {
                    patternEdit.Insert(0, new TilePatternGrid(0, 0, TilePatternType.Block));
                }
                else if (!patternEdit[centerIndex].IsBlock)
                {
                    patternEdit.RemoveAt(centerIndex);

                }

                Vector2 center = r.center;
                center.y += 15;
                center.x -= drawGridSize * 0.5f;
                center.y += drawGridSize * 0.5f;
                GUI.matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(1, -1, 1));

                DrawPattern(patternEdit, new Vector2(drawGridSize, drawGridSize));

                int minGrid = -gridSize / 2, maxGrid = gridSize / 2;
                if (gridSize % 2 == 0)
                    maxGrid--;

                var oldMat = GUI.matrix;
                GUI.matrix *= Matrix4x4.TRS(new Vector3(-(gridSize) * drawGridSize * 0.5f + drawGridSize * 0.5f, -(gridSize) * drawGridSize * 0.5f + drawGridSize * 0.5f), Quaternion.identity, Vector3.one);
                gridColor.GUIDrawGrid(new Vector2Int(gridSize, gridSize), new Vector2(drawGridSize, drawGridSize), 1f);
                GUI.matrix = oldMat;


                if (Event.current.type == EventType.MouseDown)
                {
                    var grid = ScreenPosToGrid(center, Event.current.mousePosition);
                    //Debug.Log(grid);
                    if (!(grid.x == 0 && grid.y == 0))
                    {
                        if (minGrid <= grid.x && grid.x <= maxGrid && minGrid <= grid.y && grid.y <= maxGrid)
                        {

                            var index = patternEdit.FindIndex(grid.x, grid.y);
                            if (index >= 0)
                            {
                                switch (patternEdit[index].Type)
                                {
                                    case TilePatternType.Block:

                                        patternEdit.RemoveAt(index);
                                        patternEdit.Insert(0, new TilePatternGrid(grid.x, grid.y, TilePatternType.Include));
                                        break;
                                    case TilePatternType.Include:

                                        patternEdit.RemoveAt(index);
                                        patternEdit.Insert(0, new TilePatternGrid(grid.x, grid.y, TilePatternType.Exclude));
                                        break;
                                    case TilePatternType.Exclude:

                                        patternEdit.RemoveAt(index);
                                        break;

                                }
                            }
                            else
                            {
                                patternEdit.Insert(0, new TilePatternGrid(grid.x, grid.y, TilePatternType.Block));
                            }
                            Repaint();
                        }
                        else
                        {

                            var index = patternEdit.FindIndex(grid.x, grid.y);
                            if (index >= 0)
                            {
                                patternEdit.RemoveAt(index);
                                Repaint();
                            }
                        }
                    }

                }
                GUI.matrix = Matrix4x4.identity;

                Rect rect = new Rect(r);
                using (new GUILayout.AreaScope(new Rect(r.x + 10, r.y + 30, 120, 100)))
                {
                    //using (new GUILayout.HorizontalScope())
                    //{
                    //    GUILayout.Label("size");
                    //    //  gridSize = EditorGUILayout.IntSlider(gridSize, 3, 5);
                    //    gridSize = 5;
                    //}
                    using (new GUILayout.HorizontalScope())
                    {
                        //GUILayout.Label("type");

                        tileTypeEdit = (TileType)EditorGUILayout.EnumPopup(tileTypeEdit);
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        //GUILayout.Label("preset");
                        //TileType tileType = (TileType)0;
                        //tileType = (TileType)EditorGUILayout.EnumPopup(tileType);
                        //if (tileType != 0)
                        //{
                        //    var tpl = TilePattern.DefaultPattern[tileType];
                        //    patternEdit.Set(tpl);
                        //}
                        if (GUILayout.Button("default"))
                        {
                            var tpl = TilePattern.DefaultPattern[tileTypeEdit];
                            patternEdit.Set(tpl);
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("cancel"))
                        {
                            patternEdit = null;
                            patternItemEdit = null;
                        }
                        if (GUILayout.Button("apply"))
                        {

                            if (patternEdit.Equals(TilePattern.DefaultPattern[patternItemEdit.tileType]))
                            {
                                patternItemEdit.usePattern = false;
                                patternItemEdit.pattern = null;

                            }
                            else
                            {
                                patternItemEdit.usePattern = true;
                                patternItemEdit.pattern = patternEdit;
                            }
                            patternItemEdit.tileType = tileTypeEdit;
                            patternEdit = null;
                            patternItemEdit = null;
                        }
                        GUILayout.FlexibleSpace();
                    }
                }

                return;
            }


            bool fog = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            previewRender.BeginPreview(r, background);
            previewRender.ambientColor = TilePreviewSettings.Instance.ambientColor;
            previewRender.lights[0].intensity = TilePreviewSettings.Instance.lightIntensity;


            Camera camera = previewRender.camera;

            if (previewRoot)
            {
                var cameraTarget = previewRoot;
                camera.transform.position = cameraTarget.position + camera.transform.rotation * Vector3.forward * -distance;
            }
            camera.Render();
            previewRender.EndAndDrawPreview(r);

            Unsupported.SetRenderSettingsUseFogNoDirty(fog);


            OnGUIDrag(r);
        }



        public override void OnPreviewSettings()
        {
            //if (GUILayout.Button("G"))
            //{
            //    showGrid = !showGrid;
            //}

        }

        void NextPreview()
        {

            selectedPreviewIndex = (selectedPreviewIndex + 1) % PreviewCount;
            SetPreviewDiry();
        }

        void OnGUIDrag(Rect rect)
        {
            Event e = Event.current;

            if (isDraging)
            {
                if (e.type == EventType.MouseDrag)
                {
                    Vector3 delta = e.mousePosition - dragStartPos;
                    delta *= dragSpeed;
                    if (delta.sqrMagnitude > 0.001f)
                    {
                        var angles = new Vector3(delta.y, delta.x);

                        Camera camera = previewRender.camera;
                        camera.transform.rotation = camera.transform.rotation * Quaternion.Euler(angles);

                        dragStartPos = e.mousePosition;
                        Repaint();
                    }
                    e.Use();
                }
                else if (e.type == EventType.MouseUp)
                {
                    isDraging = false;
                    e.Use();
                }
            }
            else
            {
                if (e.type == EventType.MouseDown)
                {
                    if (!isDraging && rect.Contains(e.mousePosition) && e.button == 0)
                    {
                        dragStartPos = e.mousePosition;
                        isDraging = true;
                        e.Use();
                    }
                    else if (e.button == 1)
                    {
                        NextPreview();
                    }
                }
            }

            if (e.type == EventType.ScrollWheel)
            {
                distance += e.delta.y * size * 0.05f;
                distance = Mathf.Clamp(distance, size * 0.3f, size * 3);

                e.Use();
            }

        }



    }
}
