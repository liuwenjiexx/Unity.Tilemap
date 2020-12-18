using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Tilemaps
{
    public class PathAlgorithm : BlockAlgorithm
    {
        public Vector2Int start;
        public Vector2Int end;
        public int width;

        public override void Generate(TilemapData map, TilemapData mask, out Vector3 startPosition, out Vector3 endPosition)
        {
            startPosition = new Vector3(start.x, 0, start.y);
            endPosition = new Vector3(end.x, 0, end.y);

            FillLine(map, start.x, start.y, end.x, end.y, width); 
        }
         

        void FillLine(TilemapData map, int x0, int y0, int x1, int y1, int lineWidth)
        {
            //FillLine2(map, x0, y0, x1, y1, lineWidth);

            Vector3 start = new Vector3(x0, 0f, y0);
            Vector3 end = new Vector3(x1, 0f, y1);

            Vector3 dir = end - start;
            float length = dir.magnitude;
            Quaternion rot = Quaternion.Euler(0, -90, 0) * Quaternion.LookRotation(dir, Vector3.up);
            Vector3 tangent = (Quaternion.Euler(0, 90, 0) * rot) * Vector3.forward;
            Vector3 _start = start + tangent * (-lineWidth * 0.5f);
            Rect rect = new Rect(0, 0, length, lineWidth);
            Matrix4x4 mat;
            mat = Matrix4x4.TRS(new Vector3(_start.x, 0, _start.z), rot, Vector3.one);
            map.FillRect(mat, rect, TilemapData.BLOCK);
        }

        void FillLine2(TilemapData map, int x0, int y0, int x1, int y1, int lineWidth)
        {
            Vector3 start = new Vector3(x0, 0f, y0);
            Vector3 end = new Vector3(x1, 0f, y1);
            Vector3 dir = end - start;
            Vector3 normal = (Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(dir)) * Vector3.forward;
            Vector3 _start = start + normal * (-lineWidth * 0.5f);
            Vector3 _end = end + normal * (-lineWidth * 0.5f);
            _start.x = Mathf.RoundToInt(_start.x);
            _start.z = Mathf.RoundToInt(_start.z);
            _end.x = Mathf.RoundToInt(_end.x);
            _end.z = Mathf.RoundToInt(_end.z);
            for (float i = 0; i < lineWidth; i += 0.1f)
            {
                var p = _start + normal * i;
                var p1 = _end + normal * i;
                FillLine(map, Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.z), Mathf.RoundToInt(p1.x), Mathf.RoundToInt(p1.z));
            }
        }


        void FillLine(TilemapData map, int x0, int y0, int x1, int y1)
        {
            int dy = (int)(y1 - y0);
            int dx = (int)(x1 - x0);
            int stepX, stepY;

            if (dx < 0)
            {
                dx = -dx;
                stepX = -1;
            }
            else
            {
                stepX = 1;
            }
            if (dy < 0)
            {
                dy = -dy;
                stepY = -1;
            }
            else
            {
                stepY = 1;
            }

            dx <<= 1;
            dy <<= 1;

            float fraction = 0;

            if (map.IsValidPoint(x0, y0))
                map.SetBlock(x0, y0);
            if (dx > dy)
            {
                fraction = dy - (dx >> 1);

                while (Mathf.Abs(x0 - x1) > 1)
                {
                    if (fraction >= 0)
                    {
                        y0 += stepY;
                        fraction -= dx;
                    }
                    x0 += stepX;
                    fraction += dy;
                    if (map.IsValidPoint(x0, y0))
                        map.SetBlock(x0, y0);
                }
            }
            else
            {
                fraction = dx - (dy >> 1);
                while (Mathf.Abs(y0 - y1) > 1)
                {
                    if (fraction >= 0)
                    {
                        x0 += stepX;
                        fraction -= dy;
                    }
                    y0 += stepY;
                    fraction += dx;
                    if (map.IsValidPoint(x0, y0))
                        map.SetBlock(x0, y0);
                }
            }
        }


    }
}