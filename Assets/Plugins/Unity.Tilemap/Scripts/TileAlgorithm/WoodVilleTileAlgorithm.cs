using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace Unity.Tilemaps
{
    /// <summary>
    /// 三消游戏类型，间隔块底图生成
    /// </summary>
    public class WoodVilleTileAlgorithm //: DefaultTileAlgorithm
    {
        public float scale = 1f;
        [NonSerialized]
        public Vector3 offset;
        public static int tileBlockSize = 2;
        [NonSerialized]
        public Transform root;
        /// <summary>
        /// pattern:{0},{1}
        /// </summary>
        public string parentNameFormat;
        [NonSerialized]
        public TilemapCreator creator;

        /*
        
        public override List<GameObject> Generate(Tilemap tilemap, TilemapData map, Transform parent, TilemapLayer layer, TileGroup[] tiles, float scale)
        {
            if (!creator)
                creator = GameObject.FindObjectOfType<TilemapCreator>();

            var list = base.Generate(tilemap, map, parent, layer, tiles, scale);

            var localToWorldMatrix = root.localToWorldMatrix;
            var worldToLocalMatrix = root.worldToLocalMatrix;
            Vector3 scale3 = Vector3.one * scale;

            foreach (Transform child in list.Select(o => o.transform))
            {
                var grid = creator.WorldPositionToGrid(child.position);
                int x1 = grid.x / tileBlockSize, y1 = grid.y / tileBlockSize;
                Vector3 localOffset = creator.GridToLocalPosition(grid.x % tileBlockSize, grid.y % tileBlockSize); // -(creator.GridToLocalPosition(x1, y1) - child.localPosition;
                                                                                                                   //Debug.Log(grid + " => " + x1 + ", " + y1 + " =>" + localOffset);
                Transform newParent = null;

                Vector3 position = child.localPosition;
                Vector3 angles = child.localEulerAngles;

                if (!string.IsNullOrEmpty(parentNameFormat))
                {
                    string parentName = string.Format(parentNameFormat, x1, y1);
                    newParent = root.Find(parentName);
                }
                else
                {
                    foreach (Transform c1 in root)
                    {
                        Vector2Int parentGrid;
                        parentGrid = TilemapCreator.WorldPositionToGrid(c1.position, worldToLocalMatrix, this.scale * tileBlockSize, this.offset);
                        if (parentGrid.x == x1 && parentGrid.y == y1)
                        {
                            newParent = c1;
                            break;
                        }
                    }
                }

                Vector3 offset = -new Vector3(scale, 0, scale) + localOffset;
                if (CreateTileObjectCallback != null)
                {
                    CreateTileObjectCallback(tilemap, x1, y1, newParent, child.gameObject, offset, angles, scale3);
                }
                else
                {
                    if (newParent)
                    {
                        position = TilemapCreator.GridToLocalPosition(new Vector2Int(x1, y1), this.scale * tileBlockSize) - newParent.localPosition;
                        //Debug.Log(new Vector2Int(x1, y1) + ", => " + TilemapCreator.GridToLocalPosition(new Vector2Int(x1, y1), this.scale * tileBlockSize));
                        //   position -= new Vector3(this.scale * x1, 0, this.scale * y1);
                        //position -= TilemapCreator.GridToWorldPosition(new Vector2Int(x1, y1), localToWorldMatrix, this.scale * interval, this.offset) - newParent.position;

                        position += offset;
                        child.parent = newParent;
                    }

                    child.localPosition = position;
                    child.localEulerAngles = angles;
                }
            }

            return list;
        }*/

        //public override Transform CreateTileObject(Tilemap tilemap, int x, int y, Transform parent, TileItem tileItem, Vector3 offset, Vector3 rotation, Vector3 scale)
        //{
        //    int x1 = grid.x / tileBlockSize, y1 = grid.y / tileBlockSize;
        //    return base.CreateTileObject(tilemap, x, y, parent, tileItem, offset, rotation, scale);
        //}



        //public static void Generate(int[] map, int[] layerValues, TilemapCreator tilemapCreator, Transform root)
        //{
        //    foreach (var layer in tilemapCreator.Tilemap.layers)
        //    {
        //        var tileAlgorithm = layer.tileAlgorithm as WoodVilleTileAlgorithm;
        //        tileAlgorithm.root = root;
        //        tileAlgorithm.creator = tilemapCreator;
        //    }

        //    int tileBlockSize = 2;
        //    int mapLength = tilemapCreator.Width * tilemapCreator.Height;



        //    tilemapCreator.Clear();


        //    tilemapCreator.BuildTileObject();

        //}

    }
}