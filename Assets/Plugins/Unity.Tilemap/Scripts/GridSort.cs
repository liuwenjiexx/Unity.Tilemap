using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
namespace Unity.Tilemaps
{
    public class GridSort : MonoBehaviour
    {

        public Vector2Int grid = new Vector2Int(10, 10);
        public Vector2 cellSize = Vector2.one;
        public bool descending;
        public bool flipX;
        public bool flipY;
        public bool awakeSort;
        public SortMethod sortMethod;

        private bool started;


        public enum SortMethod
        {
            None,
            Number,
        }

        private void Awake()
        {
            if (awakeSort && started)
                Sort();
        }

        private void Start()
        {
            started = true;
            Sort();
        }


        [ContextMenu("Sort")]
        public void Sort()
        {
            Transform root = transform;

            List<Transform> items = new List<Transform>();
            foreach (Transform child in root)
            {
                items.Add(child);
            }
            if (sortMethod == SortMethod.Number)
            {
                items.Sort(new NumberSort() { descending=descending});
            }
            else
            {
                if (descending)
                    items = items.OrderByDescending(o => o.name).ToList();
                else
                    items = items.OrderBy(o => o.name).ToList();
            }
            int rowCount = items.Count / grid.x, colCount = grid.x;

            for (int i = 0; i < items.Count; i++)
            {
                int x = i % colCount;
                int y = i / colCount;
                if (flipX)
                    x = colCount - 1 - x;
                if (flipY)
                    y = rowCount - 1 - y;
                Vector3 pos = new Vector3((x + 0.5f) * cellSize.x, 0, (y + 0.5f) * cellSize.y);
                items[i].localPosition = pos;
 
            }

        }


        class NumberSort : IComparer<Transform>
        {
            public bool descending;
            static Regex numRegex = new Regex("(\\d+)");

            int GetNumber(string input)
            {
                var m = numRegex.Match(input);
                if (!m.Success)
                    throw new System.Exception("not number: " + input);
                int n;
                if (!int.TryParse(m.Groups[1].Value, out n))
                    throw new System.Exception("not number: " + input);
                return n;
            }

            public int Compare(Transform x, Transform y)
            {
                int result= GetNumber(x.name) - GetNumber(y.name);
                if (descending)
                    result = -result;
                return result;
            }
        }

    }
}