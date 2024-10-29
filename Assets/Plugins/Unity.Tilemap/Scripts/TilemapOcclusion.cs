using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Tilemaps
{

    /// <summary>
    /// 添加到相机上，根据镜头区域显示 Tile
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TilemapOcclusion : MonoBehaviour
    {
        private new Camera camera;


        [Tooltip("触发范围")]
        public Vector2Int trigger = new Vector2Int(3, 3);
        [Tooltip("扩展显示范围")]
        public Vector2Int extension = new Vector2Int(1, 1);

        private bool isDiried;

        public static event Action<TilemapOcclusion, TilemapCreator> TilemapEnter;
        public static event Action<TilemapOcclusion, TilemapCreator> TilemapExit;
        public static event Action<TilemapOcclusion, TilemapCreator, BoundsInt> ViewBoundsChanged;

        public static List<TilemapOcclusion> all = new List<TilemapOcclusion>();

        public class TilemapState
        {
            public TilemapCreator tilemapCreator;
            public Dictionary<Vector2Int, List<GameObject>> cellObjects = new Dictionary<Vector2Int, List<GameObject>>();
            public BoundsInt triggerCellBounds;
            public BoundsInt cellBounds;
        }

        public List<TilemapState> states = new List<TilemapState>();


        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        void Start()
        {
            ResetBounds();
        }


        public void ResetBounds()
        {
            states.Clear();

            foreach (var tilemapCreator in TilemapCreator.all)
            {
                ResetBounds(tilemapCreator);
            }
        }

        public TilemapState ResetBounds(TilemapCreator tilemapCreator)
        {
            var viewBounds = this.states.FirstOrDefault(o => o.tilemapCreator == tilemapCreator);
            bool enter = false;
            if (viewBounds == null)
            {
                viewBounds = new TilemapState();
                viewBounds.tilemapCreator = tilemapCreator;
                tilemapCreator.ObjectAdded += TilemapCreator_ObjectAdded;
                tilemapCreator.ObjectRemoved += TilemapCreator_ObjectRemoved;
                this.states.Add(viewBounds);
                enter = true;
            }

            viewBounds.triggerCellBounds = new BoundsInt();

            if (!(viewBounds.cellBounds.position == Vector3Int.zero && viewBounds.cellBounds.size == Vector3Int.zero))
            {
                viewBounds.cellBounds = new BoundsInt();
            }

            UpdateObjects(viewBounds);

            if (enter)
            {
                TilemapEnter?.Invoke(this, tilemapCreator);
            }
            return viewBounds;
        }

        private void TilemapCreator_ObjectAdded(TilemapCreator tilemapCreator, TileObjectType objectType, GameObject go)
        {
            var state = this.states.FirstOrDefault(o => o.tilemapCreator == tilemapCreator);
            if (state != null)
            {
                var cell = tilemapCreator.WorldPositionToGrid(go.transform.position);

                if (!state.cellObjects.TryGetValue(cell, out List<GameObject> list))
                {
                    list = new List<GameObject>(0);
                    state.cellObjects[cell] = list;
                }
                list.Add(go);
                if (IsCulling(go.transform.position))
                {
                    if (go.activeSelf)
                        go.SetActive(false);
                }
                else
                {
                    if (!go.activeSelf)
                        go.SetActive(true);
                }
            }
        }

        private void TilemapCreator_ObjectRemoved(TilemapCreator tilemapCreator, TileObjectType objectType, GameObject go)
        {
            var state = this.states.FirstOrDefault(o => o.tilemapCreator == tilemapCreator);
            if (state != null)
            {
                var cell = tilemapCreator.WorldPositionToGrid(go.transform.position);

                if (state.cellObjects.TryGetValue(cell, out List<GameObject> list))
                {
                    list.Remove(go);
                }
            }
        }

        void UpdateObjects(TilemapState state)
        {
            var tilemapCreator = state.tilemapCreator;
            state.cellObjects.Clear();

            Action<Transform> updateObjects = (root) =>
            {
                List<GameObject> objs;
                if (root)
                {
                    foreach (Transform t in root)
                    {
                        var cell = tilemapCreator.WorldPositionToGrid(t.position);
                        if (!state.cellObjects.TryGetValue(cell, out objs))
                        {
                            objs = new List<GameObject>();
                            state.cellObjects[cell] = objs;
                        }
                        objs.Add(t.gameObject);
                    }
                }
            };

            for (int layerIndex = 0; layerIndex < tilemapCreator.Tilemap.layers.Length; layerIndex++)
            {
                updateObjects(tilemapCreator.GetTilesRoot(layerIndex));
                updateObjects(tilemapCreator.GetDecoratesRoot(layerIndex));
            }


            foreach (var item in state.cellObjects)
            {
                if (IsCulling(state.tilemapCreator.GridToWorldPosition(item.Key)))
                {
                    foreach (var go in item.Value)
                    {
                        if (go && go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                    }
                  
                }
                else
                {
                    foreach (var go in item.Value)
                    {
                        if (go && !go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                    }
                }
            }
        }


        private void Update()
        {
            UpdateCulling();
        }


        private Camera GetCamera()
        {
            Camera camera;
            camera = this.camera;
            return camera;
        }

        public void UpdateCulling()
        {
            foreach (var tilemapCreator in TilemapCreator.all)
            {
                if (!IsOcclusion(tilemapCreator))
                    continue;
                var cameraCellBounds = tilemapCreator.GetCameraCellBounds(GetCamera());
                cameraCellBounds = TilemapCreator.NormalizeMinMax(cameraCellBounds);

                TilemapState viewBounds = this.states.FirstOrDefault(o => o.tilemapCreator == tilemapCreator);

                if (!tilemapCreator.OverlapsCellBounds(cameraCellBounds))
                {
                    if (viewBounds != null)
                    {
                        OnTilemapExit(viewBounds);
                    }
                    continue;
                }

                if (viewBounds == null)
                {
                    viewBounds = ResetBounds(tilemapCreator);
                }

                //if (!triggerCellBounds.Contains(cameraCellBounds.min) || !triggerCellBounds.Contains(cameraCellBounds.max))
                if (isDiried || !(TilemapCreator.InCellBounds(viewBounds.triggerCellBounds, cameraCellBounds.min) && TilemapCreator.InCellBounds(viewBounds.triggerCellBounds, cameraCellBounds.max)))
                {
                    UpdateCulling(viewBounds, cameraCellBounds, isDiried);
                }
            }
        }

        private bool IsOcclusion(TilemapCreator tilemapCreator)
        {
            if (!tilemapCreator || !tilemapCreator.enabled || !tilemapCreator.occlusion || !tilemapCreator.Tilemap)
                return false;
            return true;
        }


        private void UpdateCulling(TilemapState viewBounds, BoundsInt cellBounds, bool force = false)
        {
            var orginCellBounds = viewBounds.cellBounds;
            cellBounds = TilemapCreator.NormalizeMinMax(cellBounds);

            if (cellBounds.size != Vector3Int.zero)
            {
                cellBounds.min -= (Vector3Int)trigger;
                cellBounds.max += (Vector3Int)trigger;
                cellBounds.size += new Vector3Int(1, 1, 0);
                viewBounds.tilemapCreator.ClampCellBounds(ref cellBounds);
                viewBounds.triggerCellBounds = cellBounds;

                cellBounds.min -= (Vector3Int)extension;
                cellBounds.max += (Vector3Int)extension;
                viewBounds.tilemapCreator.ClampCellBounds(ref cellBounds);
                viewBounds.cellBounds = cellBounds;
            }
            else
            {
                viewBounds.triggerCellBounds = new BoundsInt();
                viewBounds.cellBounds = new BoundsInt();
            }



            if (!force && viewBounds.cellBounds == orginCellBounds)
                return;

            Vector3Int originMin = orginCellBounds.min;
            Vector3Int originMax = orginCellBounds.max;
            for (int x = originMin.x; x < originMax.x; x++)
            {
                for (int y = originMin.y; y < originMax.y; y++)
                {
                    if (IsCulling(viewBounds.tilemapCreator.GridToWorldPosition(new Vector2Int(x, y))))
                    {
                        if (viewBounds.cellObjects.TryGetValue(new Vector2Int(x, y), out List<GameObject> objs))
                        {
                            foreach (var obj in objs)
                            {
                                if (obj.activeSelf)
                                    obj.SetActive(false);
                            }
                        }
                    }
                }
            }

            SetActive(viewBounds, viewBounds.cellBounds, true);

            isDiried = false;
            ViewBoundsChanged?.Invoke(this, viewBounds.tilemapCreator, viewBounds.cellBounds);
        }

        private void SetActive(TilemapState state, BoundsInt cellBounds, bool active)
        {
            Vector3Int min = cellBounds.min;
            Vector3Int max = cellBounds.max;
            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    if (state.cellObjects.TryGetValue(new Vector2Int(x, y), out List<GameObject> objs))
                    {
                        foreach (var obj in objs)
                        {
                            if (obj.activeSelf != active)
                                obj.SetActive(active);
                        }
                    }
                }
            }
        }

        private void OnTilemapExit(TilemapState state)
        {
            SetActive(state, state.cellBounds, false);
            state.cellBounds = new BoundsInt();
            state.triggerCellBounds = new BoundsInt();
            this.states.Remove(state);
            if (state.tilemapCreator)
            {
                state.tilemapCreator.ObjectAdded -= TilemapCreator_ObjectAdded;
                state.tilemapCreator.ObjectRemoved -= TilemapCreator_ObjectRemoved;
                TilemapExit?.Invoke(this, state.tilemapCreator);
            }
        }

        public void SetDiry()
        {
            isDiried = true;
        }

        #region Bounds


        public static bool IsCulling(Vector3 worldPos)
        {
            bool hasCulling = false;
            foreach (var occ in all)
            {
                if (!occ || !occ.enabled)
                    continue;
                hasCulling = true;
                foreach (var view in occ.states)
                {
                    var cell = view.tilemapCreator.WorldPositionToGrid(worldPos);
                    if (TilemapCreator.InCellBounds(view.cellBounds, cell))
                        return false;
                }
            }
            return hasCulling;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Camera camera = GetCamera();
            if (!camera)
                return;

            foreach (var state in states)
            {
                var tilemapCreator = state.tilemapCreator;

                DrawCellBounds(state, state.triggerCellBounds);

                DrawCellBounds(state, state.cellBounds);

                if (camera)
                {
                    Plane plane;
                    plane = new Plane(Vector3.up, Vector3.zero);
                    float dist;
                    var ray = camera.ViewportPointToRay(new Vector3(0, 0, 0));
                    Vector3 min = Vector3.zero, max = Vector3.zero;
                    if (plane.Raycast(ray, out dist))
                    {
                        min = ray.GetPoint(dist);
                    }
                    ray = camera.ViewportPointToRay(new Vector3(1f, 1f, 0));
                    if (plane.Raycast(ray, out dist))
                    {
                        max = ray.GetPoint(dist);
                    }
                    BoundsInt cellBounds = new BoundsInt();
                    cellBounds.SetMinMax((Vector3Int)tilemapCreator.WorldPositionToGrid(min), (Vector3Int)tilemapCreator.WorldPositionToGrid(max));
                    tilemapCreator.ClampCellBounds(ref cellBounds);
                    min = tilemapCreator.GridToWorldPosition((Vector2Int)cellBounds.min);
                    max = tilemapCreator.GridToWorldPosition((Vector2Int)cellBounds.max);
                    Bounds bounds = new Bounds();
                    bounds.SetMinMax(min, max);
                    var size = bounds.size;
                    size.y = 1f;
                    bounds.size = size;
                    bounds.EnumeratePoints().GizmosDrawLineStrip(new Color(1, 1, 1f, 0.3f), true);
                }
            }
        }

        void DrawCellBounds(TilemapState state, BoundsInt bounds)
        {
            var worldBounds = state.tilemapCreator.CellBoundsToWorldBounds(bounds);
            var size = worldBounds.size;
            size.y = 1f;
            worldBounds.size = size;
            worldBounds.EnumeratePoints().GizmosDrawLineStrip(new Color(1, 1, 1f, 0.3f), true);
        }

        private void OnEnable()
        {
            all.Add(this);
            TilemapCreator.Removed += TilemapCreator_Removed;

        }

        private void OnDisable()
        {
            all.Remove(this);
            TilemapCreator.Removed -= TilemapCreator_Removed;
            UpdateCulling();
        }

        void TilemapCreator_Removed(TilemapCreator tilemapCreator)
        {
            TilemapState viewBounds = this.states.FirstOrDefault(o => o.tilemapCreator == tilemapCreator);

            if (viewBounds != null)
            {
                OnTilemapExit(viewBounds);
                //this.states.Remove(viewBounds);
                //if (viewBounds.tilemapCreator)
                //    TilemapExit?.Invoke(this, viewBounds.tilemapCreator);
            }

        }


    }
}