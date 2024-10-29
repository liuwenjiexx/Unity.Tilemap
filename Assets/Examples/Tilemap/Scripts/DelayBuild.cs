using System.Collections;
using System.Collections.Generic;
using Unity.Tilemaps;
using UnityEngine;
namespace UnityEngine.Tilemaps.Example
{
    public class DelayBuild : MonoBehaviour
    {
        public float delay = 0f;
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(delay);
            GetComponent<TilemapCreator>().Build();
        } 
    }
}