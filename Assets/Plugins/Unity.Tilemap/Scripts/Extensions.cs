using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.Tilemaps
{

    static class InnerExtensions
    {
        public static Type GetTypeInAllAssemblies(this string typeName, bool ignoreCase)
        {
            Type type;
            type = Type.GetType(typeName, false, ignoreCase);
            if (type == null)
            {
                foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = ass.GetType(typeName, false, ignoreCase);
                    if (type != null)
                        break;
                }
            }
            return type;
        }
        public static Type FindType(this IEnumerable<Assembly> assemblies, string typeName, bool ignoreCase)
        {
            Type type;
            type = Type.GetType(typeName, false, ignoreCase);
            if (type == null)
            {
                foreach (var ass in assemblies)
                {
                    type = ass.GetType(typeName, false, ignoreCase);
                    if (type != null)
                        break;
                }
            }

            return type;
        }
        public static IEnumerable<Assembly> Referenced(this IEnumerable<Assembly> assemblies, IEnumerable<Assembly> referenced)
        {

            foreach (var ass in assemblies)
            {
                if (referenced.Where(o => o == ass).FirstOrDefault() != null)
                {
                    yield return ass;
                }
                else
                {
                    foreach (var refAss in ass.GetReferencedAssemblies())
                    {
                        if (referenced.Where(o => o.FullName == refAss.FullName).FirstOrDefault() != null)
                        {
                            yield return ass;
                            break;
                        }
                    }
                }
            }
        }

        public static Vector3 RandomVector3(this Vector3 min, Vector3 max)
        {
            return new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z));
        }

        public static Rect MultiplyMatrix(this Rect rect, Matrix4x4 mat)
        {
            Vector3 min, max;
            max = min = mat.MultiplyPoint(new Vector3(0, 0, 0));
            Vector3 tmp;
            tmp = mat.MultiplyPoint(new Vector3(rect.width, 0, 0));
            min = Vector3.Min(min, tmp);
            max = Vector3.Max(max, tmp);
            tmp = mat.MultiplyPoint(new Vector3(0, 0, rect.height));
            min = Vector3.Min(min, tmp);
            max = Vector3.Max(max, tmp);
            tmp = mat.MultiplyPoint(new Vector3(rect.width, 0, rect.height));
            min = Vector3.Min(min, tmp);
            max = Vector3.Max(max, tmp);
            Vector3 size = max - min;
            return new Rect(min.x, min.z, size.x, size.z);
        }

    }

    public static class Extensions
    {

        const TileFlags BlockFlags = TileFlags.Up | TileFlags.Right | TileFlags.Down | TileFlags.Left | TileFlags.UpRight | TileFlags.UpLeft | TileFlags.DownRight | TileFlags.DownLeft;
        const TileFlags EdgeUpFlags = TileFlags.Down | TileFlags.Left | TileFlags.Right | TileFlags.DownLeft | TileFlags.DownRight;
        const TileFlags EdgeDownFlags = TileFlags.Up | TileFlags.Left | TileFlags.Right | TileFlags.UpLeft | TileFlags.UpRight;
        const TileFlags EdgeRightFlags = TileFlags.Up | TileFlags.Down | TileFlags.Left | TileFlags.UpLeft | TileFlags.DownLeft;
        const TileFlags EdgeLeftFlags = TileFlags.Up | TileFlags.Down | TileFlags.Right | TileFlags.UpRight | TileFlags.DownRight;
        const TileFlags OuterCornerUpRightFlags = TileFlags.Down | TileFlags.Left | TileFlags.DownLeft;
        const TileFlags OuterCornerUpLeftFlags = TileFlags.Down | TileFlags.Right | TileFlags.DownRight;
        const TileFlags OuterCornerDownRightFlags = TileFlags.Up | TileFlags.Left | TileFlags.UpLeft;
        const TileFlags OuterCornerDownLeftFlags = TileFlags.Up | TileFlags.Right | TileFlags.UpRight;
        const TileFlags InnerCornerUpRightFlags = TileFlags.Up | TileFlags.UpRight | TileFlags.Right | TileFlags.DownRight | TileFlags.Down | TileFlags.Left | TileFlags.UpLeft;
        const TileFlags InnerCornerUpLeftFlags = TileFlags.Up | TileFlags.UpRight | TileFlags.Right | TileFlags.Down | TileFlags.DownLeft | TileFlags.Left | TileFlags.UpLeft;
        const TileFlags InnerCornerDownRightFlags = TileFlags.Up | TileFlags.UpRight | TileFlags.Right | TileFlags.DownRight | TileFlags.Down | TileFlags.DownLeft | TileFlags.Left;
        const TileFlags InnerCornerDownLeftFlags = TileFlags.Up | TileFlags.Right | TileFlags.DownRight | TileFlags.Down | TileFlags.DownLeft | TileFlags.Left | TileFlags.UpLeft;

        public static TileFlags Parse(this TileFlags tileFlags)
        {
            if (tileFlags == TileFlags.None)
                return TileFlags.None;

            if ((tileFlags & BlockFlags) == BlockFlags)
                tileFlags |= TileFlags.Block;
            else if (IsSideTileType(tileFlags, InnerCornerUpRightFlags))
                tileFlags |= TileFlags.InnerCornerUpRight;
            else if (IsSideTileType(tileFlags, InnerCornerUpLeftFlags))
                tileFlags |= TileFlags.InnerCornerUpLeft;
            else if (IsSideTileType(tileFlags, InnerCornerDownRightFlags))
                tileFlags |= TileFlags.InnerCornerDownRight;
            else if (IsSideTileType(tileFlags, InnerCornerDownLeftFlags))
                tileFlags |= TileFlags.InnerCornerDownLeft;
            else if (IsSideTileType(tileFlags, EdgeUpFlags))
                tileFlags |= TileFlags.EdgeUp;
            else if (IsSideTileType(tileFlags, EdgeDownFlags))
                tileFlags |= TileFlags.EdgeDown;
            else if (IsSideTileType(tileFlags, EdgeRightFlags))
                tileFlags |= TileFlags.EdgeRight;
            else if (IsSideTileType(tileFlags, EdgeLeftFlags))
                tileFlags |= TileFlags.EdgeLeft;
            else if (IsSideTileType(tileFlags, OuterCornerUpRightFlags))
            {
                //if (IsSideTileType(type, TileSideType.OuterCornerDownLeft))
                //    return TileSideType.None;
                tileFlags |= TileFlags.OuterCornerUpRight;
            }
            else if (IsSideTileType(tileFlags, OuterCornerUpLeftFlags))
            {
                //if (IsSideTileType(type, TileSideType.OuterCornerDownRight))
                //    return TileSideType.;
                tileFlags |= TileFlags.OuterCornerUpLeft;
            }
            else if (IsSideTileType(tileFlags, OuterCornerDownRightFlags))
                tileFlags |= TileFlags.OuterCornerDownRight;
            else if (IsSideTileType(tileFlags, OuterCornerDownLeftFlags))
                tileFlags |= TileFlags.OuterCornerDownLeft;



            return tileFlags;
        }
        static bool IsSideTileType(TileFlags sideType, TileFlags tileSideType)
        {
            if ((sideType & tileSideType) != tileSideType)
                return false;
            //if ((((sideType) & ~(tileSideType)) & TileSideType.AllSide4) != 0)
            //    return false;
            return true;
        }


        public static TileType ToTileType(this TileFlags tileFlags)
        {
            if (tileFlags == TileFlags.None)
                return TileType.Ground;

            if ((tileFlags & TileFlags.TileFlags) != 0)
            {
                if ((tileFlags & TileFlags.Block) == TileFlags.Block)
                    return TileType.Block;
                else if ((tileFlags & TileFlags.Edge) == TileFlags.Edge)
                    return TileType.Edge;
                else if ((tileFlags & TileFlags.OuterCorner) == TileFlags.OuterCorner)
                    return TileType.OuterCorner;
                else if ((tileFlags & TileFlags.InnerCorner) == TileFlags.InnerCorner)
                    return TileType.InnerCorner;
            }
            return TileType.Ground;
        }


        public static float GetOffsetAngle(this TileFlags tileFlags)
        {
            float offsetAngle = 0f;
            if ((tileFlags & TileFlags.TileFlags) != 0)
            {
                if ((tileFlags & TileFlags.Edge) == TileFlags.Edge)
                {
                    if ((tileFlags & TileFlags.EdgeRight) == TileFlags.EdgeRight)
                        offsetAngle = 90f;
                    else if ((tileFlags & TileFlags.EdgeDown) == TileFlags.EdgeDown)
                        offsetAngle = 180f;
                    else if ((tileFlags & TileFlags.EdgeLeft) == TileFlags.EdgeLeft)
                        offsetAngle = 270f;
                }
                else if ((tileFlags & TileFlags.OuterCorner) == TileFlags.OuterCorner)
                {
                    if ((tileFlags & TileFlags.OuterCornerDownRight) == TileFlags.OuterCornerDownRight)
                        offsetAngle = 90f;
                    else if ((tileFlags & TileFlags.OuterCornerDownLeft) == TileFlags.OuterCornerDownLeft)
                        offsetAngle = 180f;
                    else if ((tileFlags & TileFlags.OuterCornerUpLeft) == TileFlags.OuterCornerUpLeft)
                        offsetAngle = 270f;
                }
                else if ((tileFlags & TileFlags.InnerCorner) == TileFlags.InnerCorner)
                {
                    if ((tileFlags & TileFlags.InnerCornerUpLeft) == TileFlags.InnerCornerUpLeft)
                        offsetAngle = 90f;
                    else if ((tileFlags & TileFlags.InnerCornerUpRight) == TileFlags.InnerCornerUpRight)
                        offsetAngle = 180;
                    else if ((tileFlags & TileFlags.InnerCornerDownRight) == TileFlags.InnerCornerDownRight)
                        offsetAngle = 270f;
                }
            }

            return offsetAngle;
        }

        static System.Random random = new System.Random();

        public static int RandomIndexWithWeight<T>(this IEnumerable<T> items, Func<T, float> getWeight)
        {
            if (items == null)
                return -1;

            float totalWeight = items.Sum(o => getWeight(o));
            int index = -1;
            if (totalWeight > 0)
            {
                float value = (float)random.NextDouble();
                float weight = 0f;
                int _i = 0;
                foreach (var item in items)
                {
                    float n = getWeight(item) / totalWeight;
                    weight += n;
                    if (value < weight)
                    {
                        index = _i;
                        break;
                    }
                    _i++;
                }
            }
            return index;
        }

        public static IEnumerable<Vector3> EnumeratePoints(this Bounds bounds)
        {
            Vector3 min = bounds.min, max = bounds.max;
            yield return min;
            yield return new Vector3(max.x, min.y, min.z);
            yield return new Vector3(max.x, max.y, min.z);
            yield return new Vector3(min.x, max.y, min.z);
            yield return new Vector3(min.x, min.y, max.z);
            yield return new Vector3(max.x, min.y, max.z);
            yield return max;
            yield return new Vector3(min.x, max.y, max.z);
        }
        public static Vector3[] ToPoints(this Bounds bounds)
        {
            Vector3[] points = new Vector3[8];
            ToPoints(bounds, points, 0);
            return points;
        }
        public static void ToPoints(this Bounds bounds, Vector3[] points, int arrayOffset)
        {
            Vector3 min = bounds.min, max = bounds.max;

            points[arrayOffset] = min;
            points[arrayOffset + 1] = new Vector3(max.x, min.y, min.z);
            points[arrayOffset + 2] = new Vector3(max.x, max.y, min.z);
            points[arrayOffset + 3] = new Vector3(min.x, max.y, min.z);
            points[arrayOffset + 4] = new Vector3(min.x, min.y, max.z);
            points[arrayOffset + 5] = new Vector3(max.x, min.y, max.z);
            points[arrayOffset + 6] = max;
            points[arrayOffset + 7] = new Vector3(min.x, max.y, max.z);
        }
        public static void GizmosDrawLineStrip(this IEnumerable<Vector3> points, Color color, bool closed)
        {
            Vector3 first = new Vector3();
            Vector3 last = new Vector3();
            bool isFirst = true;
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            foreach (var point in points)
            {
                if (isFirst)
                {
                    first = point;
                    isFirst = false;
                }
                else
                {
                    Gizmos.DrawLine(last, point);
                }

                last = point;
            }
            if (!isFirst && closed)
            {
                Gizmos.DrawLine(last, first);
            }
            Gizmos.color = oldColor;
        }
    }
}