using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Tilemaps
{
    public class BlockMirrorTileAlgorithm : DefaultTileAlgorithm
    {


        public override Transform CreateTileObject(TilemapCreator creator, Tilemap tilemap, int x, int y, Transform parent, TileItem tileItem, Vector3 offset, Vector3 offsetAngle, Vector3 scale)
        {
            if (tileItem.tileType == TileType.Block)
            {
                if (x % 2 == 0)
                {
                    if (y % 2 == 0)
                    {
                        offsetAngle.y += 180f;
                    }
                    else
                    {
                        offsetAngle.y -= 90f;
                    }
                }
                else
                {
                    if (y % 2 == 0)
                    {
                        offsetAngle.y += 90f;
                    }
                    else
                    {

                    }
                }
            }

            return base.CreateTileObject( creator, tilemap, x, y, parent, tileItem,  offset, offsetAngle, scale);
        }
    }
}