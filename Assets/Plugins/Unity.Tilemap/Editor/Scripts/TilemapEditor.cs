using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;
using System.Reflection;
using UnityEditor.GUIExtensions;

namespace Unity.Tilemaps
{

    [CustomEditor(typeof(Tilemap))]
    public class TilemapEditor : Editor
    {

        void NewTile()
        {
            var asset = target as Tilemap;
            Undo.RecordObject(asset, "");
            var config = asset;
            TileGroup tile = new TileGroup();
            List<TileGroup> list = new List<TileGroup>();
            if (config.tiles != null)
                list.AddRange(config.tiles);
            list.Add(tile);
            config.tiles = list.ToArray();
            EditorUtility.SetDirty(asset);
        }

        void NewLayer()
        {

        }

        Tilemap Asset
        {
            get { return target as Tilemap; }
        }




        public override void OnInspectorGUI()
        {
            //var tileObjectsProp= serializedObject.FindProperty("tileObjects");
            var asset = Asset;
            //base.OnInspectorGUI();
            var config = asset;


            var property = serializedObject.GetIterator();
            using (var checker = new EditorGUI.ChangeCheckScope())
            {
                if (property.NextVisible(true))
                {
                    int depth = property.depth;
                    do
                    {

                        if (property.isArray)
                        {
                            if (property.arrayElementType.IndexOf("TileGroup") >= 0)
                            {
                                DrawTileConfigArray();
                                //GUIEventCondArray(property.Copy(), property.LabelContent());
                                continue;
                            }
                            if (property.arrayElementType.IndexOf("TilemapLayer") >= 0)
                            {
                                DrawLayers(property.Copy());
                                continue;
                            }
                        }
                        else
                        {
                            if (property.type == typeof(TilemapDataSettings).Name)
                            {
                                DrawDataSettings(property.Copy());
                                continue;
                            }
                        }

                        EditorGUILayout.PropertyField(property, true);

                    } while (property.NextVisible(false) && property.depth == depth);
                }


                if (checker.changed && !Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    serializedObject.UpdateIfRequiredOrScript();
                }
            }

            //if (config.tiles == null || config.tiles.Length == 0)
            //{
            //    NewTile();
            //}
            //if (GUILayout.Button("New Tile"))
            //{
            //    NewTile();
            //}




            //if (GUILayout.Button("New Layer"))
            //{
            //    NewLayer();
            //}

            //if (asset.tileObjects == null)
            //{
            //    asset.tileObjects = new GameObject[0, 0];
            //}
            //if (asset.array1 == null)
            //{
            //    asset.array1 = new List<bool[]>();
            //}

            //GUILayout.Label(asset.tileObjects.GetLength(0) + "," + asset.tileObjects.GetLength(1));

            //if (GUILayout.Button("add"))
            //{
            //    Undo.RecordObject(target, "");
            //    asset.tileObjects = new GameObject[asset.tileObjects.GetLength(0)+1, 0];
            //    EditorUtility.SetDirty(asset);
            //}
            //GUILayout.Label("list:"+asset.array1.Count );

            //if (GUILayout.Button("add"))
            //{
            //    Undo.RecordObject(target, "");
            //    asset.array1.Add(new bool[0]);
            //    EditorUtility.SetDirty(asset); 
            //}
            //GUILayout.Label("list:" + asset.array2.Count);

            //if (GUILayout.Button("add"))
            //{
            //    Undo.RecordObject(target, "");
            //    asset.array2.Add(false);
            //    EditorUtility.SetDirty(asset); 
            //}

        }

        int[] GetTileGroups()
        {
            return Asset.tiles.Select(o => o.group).Distinct().OrderBy(o => o).ToArray();
        }

        void DrawTileConfigArray()
        {
            var asset = Asset;
            var config = asset;

            if (config.tiles == null || config.tiles.Length == 0)
            {
                config.tiles = new TileGroup[] { new TileGroup() };
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(asset);
                    serializedObject.UpdateIfRequiredOrScript();
                }
            }

            using (var header = new EditorGUILayoutx.Scopes.FoldoutHeaderGroupScope(false, new GUIContent($"Tiles ({config.tiles.Length})")))
            {
                if (header.Visiable)
                {
                    using (var checker = new EditorGUI.ChangeCheckScope())
                    {
                        foreach (var tileGroup in GetTileGroups())
                        {
                            using (new GUILayout.VerticalScope("box"))
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label(string.Format("tile group {0}", tileGroup));
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("+", "label"))
                                    {
                                        var newItem = new TileGroup() { group = tileGroup };
                                        config.tiles = config.tiles.InsertArrayElementAtIndex(config.tiles.Length, newItem);
                                        GUI.changed = true;
                                    }
                                }
                                int displayIndex = 0;
                                for (int i = 0; i < config.tiles.Length; i++)
                                {
                                    var tile = config.tiles[i];
                                    if (tile.group != tileGroup)
                                        continue;
                                    if (GUILayout.Button("tile " + displayIndex, "label"))
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        menu.AddItem(new GUIContent("Delete Tile: " + displayIndex), false, (o) =>
                                         {
                                             int n = (int)o;
                                             config.tiles = config.tiles.DeleteArrayElementAtIndex(n);
                                             GUI.changed = true;
                                         }, i);
                                        menu.ShowAsContext();
                                    }
                                    using (new EditorGUILayoutx.Scopes.IndentLevelVerticalScope())
                                    {
                                        //using (new GUILayout.HorizontalScope())
                                        //{
                                        //    tile.tileEdge = DrawTileItems(tile, tile.tileEdge, TileType.Edge);
                                        //    tile.tileOuterCorner = DrawTileItems(tile, tile.tileOuterCorner, TileType.OuterCorner);
                                        //    tile.tileInnerCorner = DrawTileItems(tile, tile.tileInnerCorner, TileType.InnerCorner);
                                        //    tile.tileBlock = DrawTileItems(tile, tile.tileBlock, TileType.Block);
                                        //    tile.tileGround = DrawTileItems(tile, tile.tileGround, TileType.Ground);
                                        //}
                                        tile.tile = EditorGUILayout.ObjectField("Tile", tile.tile, typeof(Tile), false) as Tile;
                                        //tile.offset = EditorGUILayout.Vector3Field("Offset", tile.offset);
                                        //tile.blockOffset = EditorGUILayout.Vector3Field("Block Offset", tile.blockOffset);
                                        //tile.groundOffset = EditorGUILayout.Vector3Field("Ground Offset", tile.groundOffset);
                                        tile.weight = EditorGUILayout.DelayedFloatField("Weight", tile.weight);
                                        tile.group = EditorGUILayout.DelayedIntField("Group", tile.group);
                                        displayIndex++;
                                    }
                                }
                            }
                        }

                        if (checker.changed && !Application.isPlaying)
                        {
                            EditorUtility.SetDirty(serializedObject.targetObject);
                            serializedObject.UpdateIfRequiredOrScript();
                            GUI.changed = false;
                        }
                    }
                }
            }
        }

        static AlgorithmTypeInfo[] algorithmTypes;
        class AlgorithmTypeInfo
        {
            public GUIContent displayName;
            public Type type;
            public AlgorithmTypeInfo(Type type)
            {
                this.type = type;
                if (type == null)
                {
                    displayName = new GUIContent("None");
                }
                else
                {
                    displayName = new GUIContent(type.Name.EndsWith("Algorithm", StringComparison.InvariantCultureIgnoreCase) ? type.Name.Substring(0, type.Name.Length - 9) : type.Name);
                }
            }
        }

        static DecorateAlgorithmTypeInfo[] decorateAlgorithmTypes;

        static DecorateAlgorithmTypeInfo[] DecorateAlgorithmTypes
        {
            get
            {
                if (decorateAlgorithmTypes == null)
                {
                    decorateAlgorithmTypes = new DecorateAlgorithmTypeInfo[] { new DecorateAlgorithmTypeInfo(null) }
                    .Concat(AppDomain.CurrentDomain.GetAssemblies()
                        .Referenced(new Assembly[] { typeof(TilemapDecorateAlgorithm).Assembly })
                        .SelectMany(o => o.GetTypes())
                        .Where(o => !o.IsAbstract && o.IsSubclassOf(typeof(TilemapDecorateAlgorithm)))
                        .Select(o => new DecorateAlgorithmTypeInfo(o))
                        .OrderBy(o => o.displayName.text))
                        .ToArray();

                }
                return decorateAlgorithmTypes;
            }
        }

        class DecorateAlgorithmTypeInfo
        {
            public GUIContent displayName;
            public Type type;
            public DecorateAlgorithmTypeInfo(Type type)
            {
                this.type = type;
                if (type == null)
                {
                    displayName = new GUIContent("None");
                }
                else
                {
                    displayName = new GUIContent(type.Name);

                    if (displayName.text.EndsWith("DecorateAlgorithm", StringComparison.InvariantCultureIgnoreCase))
                        displayName.text = displayName.text.Substring(0, type.Name.Length - 17);

                    if (displayName.text.EndsWith("Algorithm", StringComparison.InvariantCultureIgnoreCase))
                        displayName.text = displayName.text.Substring(0, type.Name.Length - 9);
                }
            }
        }




        void DrawLayers(SerializedProperty layersProperty)
        {
            var asset = Asset;
            var config = asset;
            var layers = config.layers;
            if (layers == null || layers.Length == 0)
            {
                layers = new TilemapLayer[] { new TilemapLayer() { enabled = true } };
                config.layers = layers;
                GUI.changed = true;
            }

            if (algorithmTypes == null)
            {
                algorithmTypes = new AlgorithmTypeInfo[] { new AlgorithmTypeInfo(null) }
                .Concat(AppDomain.CurrentDomain.GetAssemblies()
                    .Referenced(new Assembly[] { typeof(BlockAlgorithm).Assembly })
                    .SelectMany(o => o.GetTypes())
                    .Where(o => !o.IsAbstract && o.IsSubclassOf(typeof(BlockAlgorithm)))
                    .Select(o => new AlgorithmTypeInfo(o))
                    .OrderBy(o => o.displayName.text))
                    .ToArray();

            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel($"Layers ({layers.Length})");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", "label", GUILayout.ExpandWidth(false)))
                {
                    TilemapLayer newItem = new TilemapLayer();
                    newItem.enabled = true;
                    newItem.input = new TilemapInput();
                    newItem.output = new TilemapOutput() { mapToMask = TilemapOperator.Or };

                    if (config.tiles != null && config.tiles.Length > 0)
                        newItem.tileGroup = config.tiles.First().group;
                    layers = layers.InsertArrayElementAtIndex(layers.Length, newItem);
                    config.layers = layers;
                    GUI.changed = true;
                }
            }

            int[] tileGroups = GetTileGroups();
            GUIContent[] tileGroupContents = tileGroups.Select(o => new GUIContent(o.ToString())).ToArray();

            using (new EditorGUILayoutx.Scopes.IndentLevelVerticalScope())
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    var layer = layers[i];
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUI.indentLevel--;

                        using (new GUILayout.VerticalScope("box"))
                        {
                            if (TilemapCreator.selectedLayerIndex == i)
                            {
                                GUI.backgroundColor = new Color(0.5f, 0.5f, 0f, 1f);
                            }
                            else
                            {
                                GUI.backgroundColor = Color.white;
                            }
                            if (!layer.enabled)
                            {
                                GUI.contentColor = Color.gray;
                            }
                            using (var header = new EditorGUILayoutx.Scopes.FoldoutHeaderGroupScope(false, new GUIContent($"layer {i} {layer.note}"),
                                onGUI: () =>
                                {

                                    if (GUILayout.Button("Active", GUILayout.ExpandWidth(false)))
                                    {
                                        if (TilemapCreator.selectedLayerIndex == i)
                                        {
                                            TilemapCreatorEditor.SelectLayer(-1);
                                        }
                                        else
                                        {
                                            TilemapCreatorEditor.SelectLayer(i);
                                        }
                                    }

                                    //GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("◥", "label", GUILayout.ExpandWidth(false)))
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        if (i > 0)
                                        {
                                            menu.AddItem(new GUIContent("Move Up"), false, (o) =>
                                           {
                                               int itemIndex = (int)o;
                                               layers.Swap(itemIndex, itemIndex - 1);
                                               GUI.changed = true;
                                           }, i);
                                        }
                                        else
                                        {
                                            menu.AddDisabledItem(new GUIContent("Move Up"), false);
                                        }
                                        if (i < layers.Length - 1)
                                        {
                                            menu.AddItem(new GUIContent("Move Down"), false, (o) =>
                                           {
                                               int itemIndex = (int)o;
                                               layers.Swap(itemIndex, itemIndex + 1);
                                               GUI.changed = true;
                                           }, i);
                                        }
                                        else
                                        {
                                            menu.AddDisabledItem(new GUIContent("Move Down"), false);
                                        }
                                        if (layers.Length > 0)
                                        {
                                            menu.AddItem(new GUIContent("Delete"), false, (o) =>
                                           {
                                               int itemIndex = (int)o;
                                               layers = layers.DeleteArrayElementAtIndex(itemIndex);
                                               config.layers = layers;
                                               GUI.changed = true;
                                           }, i);
                                        }
                                        else
                                        {
                                            menu.AddDisabledItem(new GUIContent("Delete"), false);

                                        }

                                        menu.ShowAsContext();
                                    }

                                }))
                            {
                                GUI.contentColor = Color.white;
                                GUI.backgroundColor = Color.white;
                                if (header.Visiable)
                                {

                                    layer.enabled = EditorGUILayout.Toggle("Enabled", layer.enabled, GUILayout.ExpandWidth(false));

                                    var old = GUI.enabled;
                                    GUI.enabled = old & layer.enabled;

                                    using (new GUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.PrefixLabel("Note");
                                        layer.note = EditorGUILayout.TextArea(layer.note ?? string.Empty, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 2 - 4));
                                    }

                                    GUILayout.Label("Input");
                                    EditorGUI.indentLevel++;
                                    layer.input.mask = (MaskOperator)EditorGUILayout.EnumPopup("Mask", layer.input.mask);
                                    layer.input.maskToMap = (TilemapOperator)EditorGUILayout.EnumPopup(new GUIContent("Mask To Map", "Mask To Map"), layer.input.maskToMap);
                                    EditorGUI.indentLevel--;

                                    int algorithmIndex = -1;
                                    if (layer.algorithm != null)
                                    {
                                        for (int j = 0; j < algorithmTypes.Length; j++)
                                        {
                                            if (algorithmTypes[j].type == layer.algorithm.GetType())
                                            {
                                                algorithmIndex = j;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int j = 0; j < algorithmTypes.Length; j++)
                                        {
                                            if (algorithmTypes[j].type == null)
                                            {
                                                algorithmIndex = j;
                                                break;
                                            }
                                        }
                                    }


                                    GUIContent[] displays;
                                    displays = algorithmTypes.Select(o => o.displayName).ToArray();

                                    int newAlgorithmIndex = EditorGUILayout.Popup(new GUIContent("Algrithm"), algorithmIndex, displays);
                                    if (newAlgorithmIndex != algorithmIndex)
                                    {
                                        if (newAlgorithmIndex < 0 || algorithmTypes[newAlgorithmIndex].type == null)
                                        {
                                            if (layer.algorithm != null)
                                            {
                                                layer.algorithm = null;
                                                GUI.changed = true;
                                            }
                                        }
                                        else
                                        {
                                            Type type = algorithmTypes[newAlgorithmIndex].type;

                                            layer.algorithm = Activator.CreateInstance(type) as BlockAlgorithm;
                                            GUI.changed = true;
                                        }
                                    }

                                    if (layer.algorithm != null && layer.algorithm.GetType() != typeof(BlockAlgorithm))
                                    {

                                        using (new GUILayout.HorizontalScope())
                                        {
                                            GUILayout.Space((EditorGUI.indentLevel + 1) * 15);
                                            using (new GUILayout.VerticalScope("box"))
                                            {
                                                var algorithmProp = layersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("algorithm");

                                                // EditorGUI.indentLevel--;
                                                //GUILayoutObject(algorithmProp);
                                                CustomFieldEditor.GUIObject(algorithmProp);

                                                // EditorGUI.indentLevel++;
                                            }

                                        }

                                    }

                                    int selectedIndex = Array.IndexOf(tileGroups, layer.tileGroup);
                                    var newIndex = EditorGUILayout.Popup(new GUIContent("Tile"), selectedIndex, tileGroupContents);
                                    if (newIndex != selectedIndex)
                                    {
                                        if (newIndex != -1)
                                            layer.tileGroup = tileGroups[newIndex];
                                        else
                                            layer.tileGroup = 0;
                                    }


                                    layer.tileAlgorithm = (TileAlgorithm)new GUIContent("Tile Algorithm").TypePopup(layer.tileAlgorithm, typeof(TileAlgorithm),
                                        (t) =>
                                        {
                                            string name = t.Name;
                                            foreach (var part in new string[] { "TileAlgorithm", "Algorithm" })
                                            {
                                                if (name.EndsWith(part, StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    name = name.Substring(0, name.Length - part.Length);
                                                    break;
                                                }
                                            }

                                            return name;
                                        });

                                    if (layer.tileAlgorithm != null && layer.tileAlgorithm.GetType() != typeof(TileAlgorithm))
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            EditorGUI.indentLevel++;
                                            //  GUILayout.Space((EditorGUI.indentLevel + 1) * 15);
                                            //  using (new GUILayout.VerticalScope("box"))
                                            {
                                                CustomFieldEditor.GUIObject(layersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("tileAlgorithm"));
                                            }
                                            EditorGUI.indentLevel--;

                                        }
                                    }
                                    layer.tileBlockSize = EditorGUILayout.DelayedIntField("Tile Block Size", layer.tileBlockSize);




                                    layer.offsetHeight = EditorGUILayout.DelayedFloatField("Offset Height", layer.offsetHeight);

                                    layer.flags = (TilemapLayerFlags)EditorGUILayout.EnumFlagsField("Flags", layer.flags);


                                    GUILayout.Label("Output");
                                    EditorGUI.indentLevel++;
                                    layer.output.mapToMask = (TilemapOperator)EditorGUILayout.EnumPopup(new GUIContent("Map To Mask", "Map To Mask"), layer.output.mapToMask);
                                    layer.output.mask = (MaskOperator)EditorGUILayout.EnumPopup("Mask", layer.output.mask);
                                    EditorGUI.indentLevel--;

                                    if (layer.decorates != null)
                                        DrawDecorates(layer, layersProperty.GetArrayElementAtIndex(i).FindPropertyRelative("decorates"));


                                    GUI.enabled = old;
                                }
                            }

                        }

                        EditorGUI.indentLevel++;
                    }
                }
            }

        }


        void DrawDecorates(TilemapLayer layer, SerializedProperty decoratesProperty)
        {
            var asset = Asset;
            var config = asset;
            var decorates = layer.decorates;


            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Decorates");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", "label", GUILayout.ExpandWidth(false)))
                {
                    DecorateConfig newItem = new DecorateConfig();
                    newItem.enabled = true;

                    if (decorates == null)
                        decorates = new DecorateConfig[] { newItem };
                    else
                        decorates = decorates.InsertArrayElementAtIndex(newItem);
                    layer.decorates = decorates;
                    GUI.changed = true;
                }
            }

            if (decorates == null || decorates.Length == 0)
            {
                return;
            }

            EditorGUI.indentLevel++;

            for (int i = 0; i < decorates.Length; i++)
            {
                var decorate = decorates[i];

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(EditorGUI.indentLevel * 15);
                    EditorGUI.indentLevel--;

                    using (new GUILayout.VerticalScope("box"))
                    {
                        using (var header = new EditorGUILayoutx.Scopes.FoldoutHeaderGroupScope(false, new GUIContent($"{i} {(decorate.enabled ? "" : "(disabled)")}"), onGUI: () =>
                        {
                            if (GUILayout.Button("◥", "label", GUILayout.ExpandWidth(false)))
                            {
                                GenericMenu menu = new GenericMenu();
                                if (i > 0)
                                {
                                    menu.AddItem(new GUIContent("Move Up"), false, (o) =>
                                    {
                                        int itemIndex = (int)o;
                                        decorates.Swap(itemIndex, itemIndex - 1);
                                        GUI.changed = true;
                                    }, i);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Move Up"), false);
                                }
                                if (i < decorates.Length - 1)
                                {
                                    menu.AddItem(new GUIContent("Move Down"), false, (o) =>
                                    {
                                        int itemIndex = (int)o;
                                        decorates.Swap(itemIndex, itemIndex + 1);
                                        GUI.changed = true;
                                    }, i);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Move Down"), false);
                                }
                                if (decorates.Length > 0)
                                {
                                    menu.AddItem(new GUIContent("Delete"), false, (o) =>
                                    {
                                        int itemIndex = (int)o;
                                        decorates = decorates.DeleteArrayElementAtIndex(itemIndex);
                                        layer.decorates = decorates;
                                        GUI.changed = true;
                                    }, i);
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Delete"), false);
                                }

                                menu.ShowAsContext();
                            }
                        }))
                        {
                            if (header.Visiable)
                            {
                                decorate.enabled = EditorGUILayout.Toggle("Enabled", decorate.enabled, GUILayout.ExpandWidth(false));
                                decorate.prefab = EditorGUILayout.ObjectField("Prefab", decorate.prefab, typeof(GameObject), false) as GameObject;
                                decorate.offset = EditorGUILayout.Vector3Field("Offset", decorate.offset);
                                decorate.offsetRotation = EditorGUILayout.Vector3Field("Offset Rotation", decorate.offsetRotation);
                                decorate.useTileRotation = EditorGUILayout.Toggle("Use Tile Rotation", decorate.useTileRotation);

                                decorate.useRandomOffset = EditorGUILayout.Toggle("Random Offset", decorate.useRandomOffset);
                                if (decorate.useRandomOffset)
                                {
                                    EditorGUI.indentLevel++;
                                    decorate.minRandomOffset = EditorGUILayout.Vector3Field("Min", decorate.minRandomOffset);
                                    decorate.maxRandomOffset = EditorGUILayout.Vector3Field("Max", decorate.maxRandomOffset);
                                    EditorGUI.indentLevel--;
                                }

                                decorate.useRandomRotation = EditorGUILayout.Toggle("Random Rotation", decorate.useRandomRotation);
                                if (decorate.useRandomRotation)
                                {
                                    EditorGUI.indentLevel++;
                                    decorate.minRandomRotation = EditorGUILayout.Vector3Field("Min", decorate.minRandomRotation);
                                    decorate.maxRandomRotation = EditorGUILayout.Vector3Field("Max", decorate.maxRandomRotation);
                                    EditorGUI.indentLevel--;
                                }

                                decorate.useRandomScale = EditorGUILayout.Toggle("Random Scale", decorate.useRandomScale);
                                if (decorate.useRandomScale)
                                {
                                    EditorGUI.indentLevel++;
                                    decorate.minRandomScale = EditorGUILayout.FloatField("Min", decorate.minRandomScale);
                                    decorate.maxRandomScale = EditorGUILayout.FloatField("Max", decorate.maxRandomScale);
                                    EditorGUI.indentLevel--;
                                }



                                int algorithmIndex = -1;

                                Type algorithmType = null;


                                if (decorate.algorithm != null)
                                {
                                    algorithmType = decorate.algorithm.GetType();

                                }
                                for (int j = 0; j < DecorateAlgorithmTypes.Length; j++)
                                {
                                    if (DecorateAlgorithmTypes[j].type == algorithmType)
                                    {
                                        algorithmIndex = j;
                                        break;
                                    }
                                }
                                GUIContent[] displays;
                                displays = DecorateAlgorithmTypes.Select(o => o.displayName).ToArray();

                                int newAlgorithmIndex = EditorGUILayout.Popup(new GUIContent("Algrithm"), algorithmIndex, displays);
                                if (newAlgorithmIndex != algorithmIndex)
                                {
                                    if (newAlgorithmIndex < 0 || DecorateAlgorithmTypes[newAlgorithmIndex].type == null)
                                    {
                                        if (decorate.algorithm != null)
                                        {
                                            decorate.algorithm = null;
                                            algorithmType = null;
                                            GUI.changed = true;
                                        }
                                    }
                                    else
                                    {
                                        algorithmType = DecorateAlgorithmTypes[newAlgorithmIndex].type;

                                        decorate.algorithm = Activator.CreateInstance(algorithmType) as TilemapDecorateAlgorithm;
                                        GUI.changed = true;
                                    }
                                }

                                if (decorate.algorithm != null && algorithmType != typeof(TilemapDecorateAlgorithm))
                                {

                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Space((EditorGUI.indentLevel + 1) * 15);
                                        using (new GUILayout.VerticalScope("box"))
                                        {
                                            var algorithmProp = decoratesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("algorithm");

                                            // EditorGUI.indentLevel--;
                                            //GUILayoutObject(algorithmProp);
                                            CustomFieldEditor.GUIObject(algorithmProp);

                                            // EditorGUI.indentLevel++;
                                        }

                                    }

                                }

                            }
                        }
                    }
                    EditorGUI.indentLevel++;
                }

            }


            EditorGUI.indentLevel--;

        }

        void DrawDataSettings(SerializedProperty dataSettingsProperty)
        {
            TilemapDataSettings dataSettings = Asset.data;

            using (var group = new EditorGUILayoutx.Scopes.FoldoutHeaderGroupScope(false, new GUIContent("Data")))
            {
                if (group.Visiable)
                {
                    dataSettings.provider = (TilemapDataProvider)new GUIContent("Provider").TypePopup(dataSettings.provider, typeof(TilemapDataProvider),
                     (t) =>
                        {
                            string name = t.Name;
                            foreach (var part in new string[] { "TilemapDataProvider", "DataProvider", "Provider" })
                            {
                                if (name.EndsWith(part, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    name = name.Substring(0, name.Length - part.Length);
                                    break;
                                }
                            }

                            return name;
                        });

                    if (dataSettings.provider != null && dataSettings.provider.GetType() != typeof(TilemapDataProvider))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            //  GUILayout.Space((EditorGUI.indentLevel + 1) * 15);
                            //  using (new GUILayout.VerticalScope("box"))
                            {
                                CustomFieldEditor.GUIObject(dataSettingsProperty.FindPropertyRelative("provider"));
                            }
                            EditorGUI.indentLevel--;

                        }
                    }
                }
            }
        }

        static void GUILayoutObject(SerializedProperty property)
        {

            int depth = property.depth + 1;
            if (property.NextVisible(true) && depth == property.depth)
            {
                do
                {
                    EditorGUILayout.PropertyField(property);
                } while (property.NextVisible(false) && depth == property.depth);
            }
        }

    }
}