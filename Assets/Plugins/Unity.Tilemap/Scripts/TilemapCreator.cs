using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

[assembly: UnityEngine.Scripting.Preserve]

namespace Unity.Tilemaps
{

    public class TilemapCreator : MonoBehaviour
    {
        [SerializeField]
        private Tilemap tilemap;
        public static int selectedLayerIndex = -1;

        public Tilemap Tilemap
        {
            get
            {
                return tilemap;
            }
            set
            {
                tilemap = value;
            }
        }

        public int Width { get { return Tilemap.width; } }

        public int Height { get { return Tilemap.height; } }



        [HideInInspector]
        public bool isDirtied;


        Transform root;
        public static TilemapData debugDrawMap;

        public Transform Root
        {
            get
            {
                if (!root)
                {
                    //var go = GameObject.Find("TilemapRoot");
                    //if (go)
                    //    tileMapRoot = go.transform;
                    //if (!tileMapRoot)
                    //{
                    //    tileMapRoot = new GameObject("TilemapRoot").transform;
                    //}
                    root = transform;
                }

                return root;
            }
        }

        public Transform LayersRoot
        {
            get
            {
                return GetLayersRoot();
            }
        }

        [NonSerialized]
        public List<GameObject> allTileObjects = new List<GameObject>();

        /// <summary>
        /// 定制实例化
        /// </summary>
        public InstantiateGameObjectDelegate InstantiateGameObject;

        public static Func<TilemapInstantiateData, GameObject> EditorInstantiateGameObject;

        public delegate GameObject InstantiateGameObjectDelegate(TilemapInstantiateData instantiateData);

        /// <summary>
        /// 定制销毁
        /// </summary>
        public DestroyGameObjectDelegate DestroyGameObject;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>返回 true 表示已处理 </returns>
        public delegate bool DestroyGameObjectDelegate(GameObject instance);

        public Transform GetLayersRoot(bool create = true)
        {
            var tileMapRoot = Root;
            var layersRoot = tileMapRoot.Find("layers");
            if (!layersRoot && create)
            {
                layersRoot = new GameObject("layers").transform;
                layersRoot.parent = tileMapRoot;
                layersRoot.transform.localPosition = Vector3.zero;
                layersRoot.transform.localEulerAngles = Vector3.zero;
                layersRoot.transform.localScale = Vector3.one;
            }
            return layersRoot;
        }

        //public Transform DecoratesRoot
        //{
        //    get
        //    {
        //        var tileMapRoot = Root;
        //        var decoratesRoot = tileMapRoot.Find("decorates");
        //        if (!decoratesRoot)
        //        {
        //            decoratesRoot = new GameObject("decorates").transform;
        //            decoratesRoot.parent = tileMapRoot;
        //            decoratesRoot.transform.localPosition = Vector3.zero;
        //            decoratesRoot.transform.localEulerAngles = Vector3.zero;
        //            decoratesRoot.transform.localScale = Vector3.one;
        //        }
        //        return decoratesRoot;
        //    }
        //}

        public TilemapLayer SelectedLayer
        {
            get
            {
                if (selectedLayerIndex >= 0 && selectedLayerIndex < Tilemap.layers.Length)
                {
                    return Tilemap.layers[selectedLayerIndex];
                }
                return null;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }
        [HideInInspector]
        [SerializeField]
        List<TilemapData> maps = new List<TilemapData>();

        public List<TilemapData> Maps
        {
            get
            {
                return maps;
            }
        }

        public TilemapData GetMap(int layer)
        {
            return maps[layer];
        }

        public static void InitalizeMap(bool[] map)
        {
            for (int i = 0, len = map.Length; i < len; i++)
            {
                map[i] = !TilemapData.BLOCK;
            }
        }

        public int GridToIndex(Vector2Int grid)
        {
            return grid.x + grid.y * Width;
        }
        public int GridToIndex(int x, int y)
        {
            int index = x + y * Width;
            return index;
        }
        public Vector3 GridToWorldPosition(Vector2Int grid)
        {
            Vector3 point = new Vector3((grid.x + 0.5f) * Tilemap.scale, 0f, (grid.y + 0.5f) * Tilemap.scale);
            return transform.TransformPoint(point);
        }
        public static Vector3 GridToWorldPosition(Vector2Int grid, Matrix4x4 localToWorldMatrix, float scale, Vector3 offset)
        {
            Vector3 point = new Vector3((grid.x + 0.5f) * scale, 0f, (grid.y + 0.5f) * scale);
            return localToWorldMatrix.MultiplyPoint(point) + offset;
        }
        public static Vector3 GridToLocalPosition(Vector2Int grid, float scale)
        {
            return GridToLocalPosition(grid.x, grid.y, scale);
        }
        public static Vector3 GridToLocalPosition(int x, int y, float scale)
        {
            Vector3 point = new Vector3((x + 0.5f) * scale, 0f, (y + 0.5f) * scale);
            return point;
        }

        public static Vector2Int WorldPositionToGrid(Vector3 position, Matrix4x4 worldToLocalMatrix, float scale, Vector3 offset)
        {
            Vector3 point = worldToLocalMatrix.MultiplyPoint(position);
            point -= offset;
            Vector2Int grid = new Vector2Int();
            if (point.x < 0)
                point.x -= scale;
            if (point.z < 0)
                point.z -= scale;
            grid.x = (int)(point.x / scale);
            grid.y = (int)(point.z / scale);
            return grid;
        }

        public Vector3 GridToWorldPosition(int x, int y)
        {
            Vector3 point = GridToLocalPosition(x, y);
            return transform.TransformPoint(point);
        }
        public Vector3 GridToLocalPosition(int x, int y)
        {
            Vector3 point = new Vector3((x + 0.5f) * Tilemap.scale, 0f, (y + 0.5f) * Tilemap.scale);
            return point;
        }

        public Vector2Int WorldPositionToGrid(Vector3 position)
        {
            Vector3 point = transform.InverseTransformPoint(position);
            Vector2Int grid = new Vector2Int();
            if (point.x < 0)
                point.x -= Tilemap.scale;
            if (point.z < 0)
                point.z -= Tilemap.scale;
            grid.x = (int)(point.x / Tilemap.scale);
            grid.y = (int)(point.z / Tilemap.scale);
            return grid;
        }
        public bool IsValidGrid(Vector2Int grid)
        {
            if (grid.x < 0 || grid.x >= Width)
                return false;
            if (grid.y < 0 || grid.y >= Height)
                return false;
            return true;
        }

        public bool ScreenPointToGrid(int layer, Vector2 screenPoint, out Vector2Int grid)
        {
            var ray = Camera.main.ScreenPointToRay(screenPoint);
            Vector3 layerOffset = GetLayerOffset(layer);
            Plane plane = new Plane(transform.up, transform.position + layerOffset);
            float dist;
            if (plane.Raycast(ray, out dist))
            {
                grid = WorldPositionToGrid(ray.GetPoint(dist));
                return true;
            }
            grid = new Vector2Int();
            return false;
        }

        public Vector3 GetLayerOffset(int layer)
        {
            return GetLayerOffset(Tilemap.layers[layer]);
        }
        public Vector3 GetLayerOffset(TilemapLayer layer)
        {
            return new Vector3(0, layer.offsetHeight, 0);
        }

        class BuildPiepline
        {
            public List<TilemapLayer> layers = new List<TilemapLayer>();
            public List<TilemapData> maps = new List<TilemapData>();

        }

        static bool MASK = TilemapData.BLOCK;


        void SetMask(TilemapData mask, MaskOperator @operator)
        {

            switch (@operator)
            {
                case MaskOperator.Clear:
                    mask.Fill(0, 0, mask.Width, mask.Height, !MASK);
                    break;
                case MaskOperator.Not:
                    for (int x = 0, width = mask.Width; x < width; x++)
                    {
                        for (int y = 0, height = mask.Height; y < height; y++)
                        {
                            mask.SetBlock(x, y, !mask.IsBlock(x, y));
                        }
                    }
                    break;

            }
        }

        public void Build()
        {
            BuildTilemapData();
            BuildTileObject();
            BuildDecorateObject();
        }
        public void Clear()
        {
            Maps.Clear();
            ClearTileObject();
            ClearDecorateObject();
        }

        public void BuildTilemapData()
        {

            var config = this.Tilemap;

            maps.Clear();

            TilemapData map;

            BuildPiepline piepline = new BuildPiepline();
            TilemapData mask = new TilemapData(config.width, config.height);
            mask.Fill(0, 0, mask.Width, mask.Height, !MASK);
            for (int layerIndex = 0; layerIndex < config.layers.Length; layerIndex++)
            {
                var layer = config.layers[layerIndex];
                map = new TilemapData(config.width, config.height);
                if (layer.enabled)
                {

                    //mask
                    if (layer.input != null && layer.input.mask != MaskOperator.None)
                    {
                        SetMask(mask, layer.input.mask);
                    }


                    if (layer.input != null && layer.input.maskToMap != TilemapOperator.None)
                    {
                        map.Set(mask, layer.input.maskToMap);
                    }

                    var algorithm = layer.algorithm;
                    if (algorithm != null && algorithm.GetType() != typeof(BlockAlgorithm))
                    {
                        Vector3 startPos, endPos;
                        algorithm.Generate(map, mask, out startPos, out endPos);
                    }

                    if (layer.output != null)
                    {
                        if (layer.output.mapToMask != TilemapOperator.None)
                        {
                            mask.Set(map, layer.output.mapToMask);
                        }

                        if (layer.output.mask != MaskOperator.None)
                        {
                            SetMask(mask, layer.output.mask);
                        }
                    }
                }
                maps.Add(map);

                piepline.maps.Add(map);
                piepline.layers.Add(layer);
            }
        }




        private void FillGround(TilemapData map, TilemapData mask)
        {
            int width = map.Width, height = map.Height;
            bool isBlock;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    isBlock = mask.IsBlock(x, y);
                    if (isBlock)
                        map.SetBlock(x, y);
                }
            }
            // FillOutline(map, !TilemapData.BLOCK);

        }




        public Transform GetLayerRoot(int layer, bool create = true)
        {
            Transform layersRoot = GetLayersRoot(create);
            if (!create)
                return null;
            string layerName = "layer_" + layer;
            Transform layerRoot;
            layerRoot = layersRoot.Find(layerName);
            if (!layerRoot && create)
            {
                layerRoot = new GameObject(layerName).transform;
                layerRoot.parent = layersRoot;
                layerRoot.localPosition = Vector3.zero;
                layerRoot.localEulerAngles = Vector3.zero;
                layerRoot.localScale = Vector3.one;
            }
            return layerRoot;
        }

        const string TilesRootName = "tiles";
        const string DecoratesRootName = "decorates";

        public Transform GetTilesRoot(int layer)
        {
            return GetRoot(layer, TilesRootName);
        }
        public Transform GetDecoratesRoot(int layer)
        {
            return GetRoot(layer, DecoratesRootName);
        }
        public Transform GetRoot(int layer, string rootName)
        {
            Transform layerRoot = GetLayerRoot(layer);
            Transform root = layerRoot.Find(rootName);
            if (!root)
            {
                root = new GameObject(rootName).transform;
                root.parent = layerRoot;
                root.localPosition = Vector3.zero;
                root.localEulerAngles = Vector3.zero;
                root.localScale = Vector3.one;
            }
            return root;
        }


        public void BuildTileObject()
        {
            var config = this.Tilemap;
            if (allTileObjects == null)
                allTileObjects = new List<GameObject>();
            ClearTileObject();

            var root = Root;
            root.transform.position = transform.position;
            root.transform.rotation = transform.rotation;
            root.transform.localScale = Vector3.one;
            var layersRoot = LayersRoot;

            for (int layerIndex = 0; layerIndex < config.layers.Length; layerIndex++)
            {
                TilemapLayer layer = config.layers[layerIndex];
                layer.layerIndex = layerIndex;
                if (!layer.enabled)
                {
                    continue;
                }
                GetLayerRoot(layerIndex).localPosition = new Vector3(0, layer.offsetHeight, 0);
                Transform tilesRoot = GetTilesRoot(layerIndex);


                var tileGroups = config.tiles.Where(o => o.group == layer.tileGroup).ToArray();

                TilemapData map;

                map = GetMap(layerIndex);
                if ((layer.flags & TilemapLayerFlags.Hide) != TilemapLayerFlags.Hide)
                {
                    TileAlgorithm tileAlgorithm = layer.tileAlgorithm;
                    if (tileAlgorithm == null || tileAlgorithm.GetType() == typeof(TileAlgorithm))
                    {
                        tileAlgorithm = new DefaultTileAlgorithm();
                    }

                    var items = tileAlgorithm.Generate(this, tilemap, map, tilesRoot, layer, tileGroups, config.scale);
                    if (items != null)
                        allTileObjects.AddRange(items);
                }
            }
        }


        public void ClearTileObject()
        {
            if (allTileObjects != null)
            {
                for (int i = 0; i < allTileObjects.Count; i++)
                {
                    var go = allTileObjects[i];
                    if (go)
                    {
                        if (DestroyGameObject == null || !DestroyGameObject(go))
                        {
                            DestroyImmediate(go);
                        }
                    }
                }
                allTileObjects.Clear();
            }
            for (int layerIndex = 0; layerIndex < Tilemap.layers.Length; layerIndex++)
            {
                var tilesRoot = GetTilesRoot(layerIndex);
                while (tilesRoot.childCount > 0)
                {
                    DestroyImmediate(tilesRoot.GetChild(0).gameObject);
                }
            }
        }



        public static TilePattern[] defaultPatterns;
        public static TilePattern[] AllDefaultPatterns()
        {
            if (defaultPatterns != null)
                return defaultPatterns;
            defaultPatterns = TilePattern.DefaultPattern.Where(o => o.Key != TileType.Ground).OrderBy(o =>
                {
                    switch (o.Key)
                    {
                        case TileType.Block:
                            return 0;
                        case TileType.InnerCorner:
                            return 1;
                        case TileType.Edge:
                            return 2;
                        case TileType.OuterCorner:
                            return 3;
                        case TileType.Ground:
                            return 4;
                    }
                    return 0;
                }).SelectMany(o =>
                {
                    var pattern = o.Value;
                    List<TilePattern> ps = new List<TilePattern>();
                    ps.Add(pattern);
                    if (o.Key != TileType.Ground)
                    {
                        for (int i = 0; i < pattern.Length; i++)
                        {
                            if (pattern[i].IsBlock)
                            {
                                var grid = pattern[i];
                                Vector2Int offset = new Vector2Int(-grid.X, -grid.Y);
                                if (!(offset.x == 0 && offset.y == 0))
                                    ps.Add(pattern.Rotate(offset.x, offset.y, 0));

                                ps.Add(pattern.Rotate(offset.x, offset.y, 90));
                                ps.Add(pattern.Rotate(offset.x, offset.y, 180));
                                ps.Add(pattern.Rotate(offset.x, offset.y, 270));
                            }
                        }
                    }
                    return ps;
                }).ToArray();
            return defaultPatterns;
        }








        public void BuildDecorateObject()
        {
            var config = this.Tilemap;

            ClearDecorateObject();

            for (int layerIndex = 0; layerIndex < config.layers.Length; layerIndex++)
            {
                var layer = config.layers[layerIndex];
                if (!layer.enabled)
                    continue;
                if (layer.decorates == null)
                    continue;
                var decoratesRoot = GetDecoratesRoot(layerIndex);
                var map = GetMap(layerIndex);
                for (int j = 0; j < layer.decorates.Length; j++)
                {
                    var decorateConfig = layer.decorates[j];
                    if (decorateConfig == null || !decorateConfig.enabled || decorateConfig.algorithm == null)
                        continue;
                    var algorithm = decorateConfig.algorithm;
                    algorithm.Generate(this, layerIndex, map, decorateConfig);
                }
            }


        }

        public void ClearDecorateObject()
        {
            for (int layerIndex = 0; layerIndex < Tilemap.layers.Length; layerIndex++)
            {
                var decoratesRoot = GetDecoratesRoot(layerIndex);
                while (decoratesRoot.childCount > 0)
                {
                    DestroyImmediate(decoratesRoot.GetChild(0).gameObject);
                }
            }

        }



        [Serializable]
        class TilemapSerialization
        {
            [SerializeField]
            TilemapBitsSerialization[] datas;


            public TilemapSerialization(TilemapData[] datas)
            {
                this.datas = datas.Select(o => new TilemapBitsSerialization(o)).ToArray();
            }

            [Serializable]
            class TilemapBitsSerialization
            {
                [SerializeField]
                public int width;
                [SerializeField]
                public int height;
                [SerializeField]
                public ulong[] ulongBits;


                private const int BIT_WIDTH = 64;

                public TilemapBitsSerialization(TilemapData data)
                {
                    this.width = data.Width;
                    this.height = data.Height;

                    int bitsColumn = Mathf.CeilToInt(width / (float)BIT_WIDTH);
                    this.ulongBits = new ulong[bitsColumn * height];

                    bool isBlock;
                    int index;
                    int bits;
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            isBlock = data.IsBlock(x, y);
                            index = GridToIndex(x, y, bitsColumn, out bits);
                            if (isBlock)
                            {
                                this.ulongBits[index] = this.ulongBits[index] | (1ul << bits);
                            }
                        }
                    }

                }

                //public static TilemapBitsSerialization CreateLongBits(til



                int GridToIndex(int x, int y, int bitsColumn, out int bits)
                {
                    int xIndex = x / BIT_WIDTH;
                    bits = x % BIT_WIDTH;
                    int index = bitsColumn * y + xIndex;
                    return index;
                }

                public TilemapData ToTilemapData()
                {
                    TilemapData data = new TilemapData(width, height);
                    int bitsColumn = Mathf.CeilToInt(width / (float)BIT_WIDTH);

                    bool isBlock;
                    int index;
                    int bits;
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            index = GridToIndex(x, y, bitsColumn, out bits);
                            isBlock = (this.ulongBits[index] & (1ul << bits)) != 0;
                            data.SetBlock(x, y, isBlock);
                        }
                    }

                    return data;
                }

            }

            public TilemapData[] ToTilemapDatas()
            {
                return this.datas.Select(o => o.ToTilemapData()).ToArray();
            }

        }



        public string ToJson()
        {
            TilemapSerialization obj = new TilemapSerialization(maps.ToArray());

            return JsonUtility.ToJson(obj);
        }

        public void LoadFromJson(string json)
        {
            var obj = JsonUtility.FromJson<TilemapSerialization>(json);
            this.maps = obj.ToTilemapDatas().ToList();

        }


        public void LoadLayerValueData(int[] map, int dataBlockSize, int[] layerValues)
        {
            bool[] layerBlocks = new bool[layerValues.Length];
            int[] tileBlockSize = new int[layerValues.Length];
            for (int i = 0; i < layerBlocks.Length; i++)
            {
                layerBlocks[i] = true;
                tileBlockSize[i] = dataBlockSize;
            }
            LoadLayerValueData(map, dataBlockSize, layerValues, layerBlocks, tileBlockSize);
        }
        public void LoadLayerValueData(int[] map, int dataBlockSize, int[] layerValues, bool[] layerBlocks, int[] tileBlockSize)
        {
            Maps.Clear();
            int row = (int)(Height / dataBlockSize);
            int col = (int)(Width / dataBlockSize);

            for (int layerIndex = 0; layerIndex < Tilemap.layers.Length; layerIndex++)
            {
                TilemapData map2 = new TilemapData(Width, Height);

                int value = layerValues[layerIndex];

                if (!layerBlocks[layerIndex])
                    map2.SetBlock(0, 0, map2.Width, map2.Height);

                for (int y = 0; y < row; y++)
                {
                    for (int x = 0; x < col; x++)
                    {

                        if (map[y * col + x] == value)
                        {
                            int x2 = (x * dataBlockSize);
                            int y2 = (y * dataBlockSize);
                            map2.Fill(x2, y2, tileBlockSize[layerIndex], tileBlockSize[layerIndex], layerBlocks[layerIndex]);
                        }
                    }
                }

                Maps.Add(map2);
            }
        }

    }


    public struct TilemapInstantiateData
    {
        public int x;
        public int y;
        public GameObject prefab;
        public Transform parent;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }


    [Serializable]
    public class TemplateData
    {
        public bool[] data;

        private int width;
        private int height;

        public TemplateData(int width, int height)
        {
            if (width < 0)
                width = 0;
            if (height < 0)
                height = 0;
            this.width = width;
            this.height = height;
            data = new bool[width * height];
            TilemapCreator.InitalizeMap(data);
        }

        public bool this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Width { get => width; }
        public int Height { get => height; }



    }





}