using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace Unity.Tilemaps
{

    [Serializable]
    public class TileAlgorithm
    {
        [NonSerialized]
        public CreateTileObjectCallbackDelagate CreateTileObjectCallback;

        public virtual List<GameObject> Generate(TilemapCreator creator, Tilemap tilemap, TilemapData map, Transform parent, TilemapLayer layer, TileGroup[] tiles, float scale)
        {
            return new List<GameObject>();
        }

        public virtual Transform CreateTileObject(TilemapCreator creator, Tilemap tilemap, int x, int y, Transform parent, TileItem tileItem, Vector3 offset, Vector3 rotation, Vector3 scale)
        {
            //    return CreateTileObject(x, y, parent, tileItem, offset, rotation, scale);
            //}

            //public Transform CreateTileObject(int x, int y, Transform parent, TileItem tileItem, float offsetAngle, float scale)
            //{
            //    return CreateTileObject(x, y, parent, tileItem, Vector3.zero, new Vector3(0, offsetAngle, 0), Vector3.one * scale);
            //}
            //public Transform CreateTileObject(int x, int y, Transform parent, TileItem tileItem, Vector3 offset, Vector3 offsetAngle, Vector3 scale)
            //{
            //if (tileItem == null || !tileItem.prefab)


            GameObject prefab = null;
            if (!tileItem.IsEmpty)
            {
                int prefabIndex = tileItem.items.RandomIndexWithWeight(o => o.weight);
                if (prefabIndex >= 0)
                {
                    var tilePrefab = tileItem.items[prefabIndex];
                    prefab = tilePrefab.prefab;
                    if (tilePrefab.rotation)
                    {
                        rotation += new Vector3(0f, 90f * UnityEngine.Random.Range(0, 4), 0f);
                    }
                }
            }
            if (!prefab)
                return null;
            GameObject go;
            Vector3 tileOffset = tileItem.offset + offset;
            Vector3 position = new Vector3(x + 0.5f, 0, y + 0.5f);
            position = Vector3.Scale((position + tileOffset), scale);
            rotation += tileItem.rotation;



            if (Application.isPlaying && (!creator || creator.InstantiateGameObject == null))
            {
                go = Object.Instantiate(prefab);
                go.transform.parent = parent;
                //  offset.y += layer.offsetHeight;
                //if (tileType == TileType.Ground)
                //    offset += tileGroup.groundOffset;
                //else if (tileType == TileType.Block)
                //    offset += tileGroup.blockOffset;

                go.transform.localPosition = position;
                go.transform.localEulerAngles = rotation;
                go.transform.localScale = Vector3.Scale(go.transform.localScale, scale);
            }
            else
            {

                TilemapInstantiateData instantiateData = new TilemapInstantiateData()
                {
                    x = x,
                    y = y,
                    parent = parent,
                    prefab = prefab,
                    position = position,
                    rotation = rotation,
                    scale = scale,
                    objectType = TileObjectType.Tile
                };

                if (Application.isPlaying)
                {
                    go = creator.InstantiateGameObject(instantiateData);
                }
                else
                {
                    go = TilemapCreator.EditorInstantiateGameObject(instantiateData);
                }
            }
            if (go)
            {
                if (creator)
                    creator.AddObject(TileObjectType.Tile, go);
                return go.transform;
            }
            return null;
        }

    }



    public delegate GameObject CreateTileObjectCallbackDelagate(int x, int y, Transform parent, GameObject tileObject, Vector3 offset, Vector3 rotation, Vector3 scale);


}