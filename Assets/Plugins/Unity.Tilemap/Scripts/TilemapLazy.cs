using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Tilemaps;
using UnityEngine;
namespace UnityEngine.Tilemaps
{

    /// <summary>
    /// 需要和 <see cref="TilemapOcclusion"/> 配合使用按需加载
    /// </summary>
    [RequireComponent(typeof(TilemapCreator))]
    public class TilemapLazy : MonoBehaviour
    {
        TilemapCreator tilemapCreator;
        Dictionary<Vector2Int, CellState> cellStates = new Dictionary<Vector2Int, CellState>();
        private bool dirtied;

        private void Awake()
        {
            tilemapCreator = GetComponent<TilemapCreator>();
            tilemapCreator.InstantiateGameObject += InstantiateGameObject;
            tilemapCreator.ObjectRemoved += TilemapCreator_ObjectRemoved;
            TilemapOcclusion.ViewBoundsChanged += TilemapOcclusion_ViewBoundsChanged;

        }

        private void TilemapCreator_ObjectRemoved(TilemapCreator arg1, TileObjectType arg2, GameObject go)
        {
            var cell = tilemapCreator.WorldPositionToGrid(go.transform.position);
            if (cellStates.TryGetValue(cell, out CellState state))
            {
                state.gameObjects.Remove(go);
            }
        }

        private void TilemapOcclusion_ViewBoundsChanged(TilemapOcclusion occlusion, TilemapCreator creator, BoundsInt cellBounds)
        {
            LoadBounds(cellBounds);
        }

        void LoadBounds(BoundsInt cellBounds)
        {
            for (int x = cellBounds.xMin; x < cellBounds.xMax; x++)
            {
                for (int y = cellBounds.yMin; y < cellBounds.yMax; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (!TilemapOcclusion.IsCulling(tilemapCreator.GridToWorldPosition(cell)))
                    {
                        if (cellStates.TryGetValue(cell, out CellState state))
                        {
                            if (!state.loaded)
                            {
                                foreach (var instantiateData in state.instantiateDatas)
                                {
                                    GameObject go = Instantiate(instantiateData.prefab, instantiateData.parent);
                                    go.transform.localPosition = instantiateData.position;
                                    go.transform.localEulerAngles = instantiateData.rotation;
                                    go.transform.localScale = Vector3.Scale(go.transform.localScale, instantiateData.scale);
                                    state.gameObjects.Add(go);
                                    tilemapCreator.AddObject(instantiateData.objectType, go);
                                }
                                state.loaded = true;
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (dirtied)
            {
                dirtied = false;
                LoadBounds(new BoundsInt(0, 0, 0, tilemapCreator.Width, tilemapCreator.Height, 0));
            }
        }

        GameObject InstantiateGameObject(TilemapInstantiateData instantiateData)
        {
            var cell = new Vector2Int(instantiateData.x, instantiateData.y);
            if (!cellStates.TryGetValue(cell, out CellState cellState))
            {
                cellState = new CellState()
                {
                    cell = cell,

                };
                cellStates.Add(cell, cellState);
            }
            cellState.instantiateDatas.Add(instantiateData);
            dirtied = true;
            return null;
        }



        private void OnDestroy()
        {
            TilemapOcclusion.ViewBoundsChanged -= TilemapOcclusion_ViewBoundsChanged;
        }

        class CellState
        {
            public Vector2Int cell;
            public bool loaded;
            public List<TilemapInstantiateData> instantiateDatas = new List<TilemapInstantiateData>();
            public List<GameObject> gameObjects = new List<GameObject>();
        }

    }
}