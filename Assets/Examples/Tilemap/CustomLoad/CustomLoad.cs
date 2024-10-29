using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Tilemaps
{
    [RequireComponent(typeof(TilemapCreator))]
    public class CustomLoad : MonoBehaviour
    {
        private TilemapCreator tilemapCreator;

        private void Awake()
        {
            tilemapCreator = GetComponent<TilemapCreator>();
 
        }
 
        void Start()
        {
            Generate();
        }

        public void Generate()
        {
            tilemapCreator.InstantiateGameObject = InstantiateGameObject;
            tilemapCreator.DestroyGameObject = InstantiateGameObject;

            tilemapCreator.Build();
        }
        GameObject InstantiateGameObject(TilemapInstantiateData instantiateData)
        {
            GameObject go = Instantiate(instantiateData.prefab, instantiateData.parent);
            go.transform.localPosition = instantiateData.position;
            go.transform.localEulerAngles = instantiateData.rotation;
            go.transform.localScale = Vector3.Scale(go.transform.localScale, instantiateData.scale);

            return go;
        }

        bool InstantiateGameObject(GameObject go)
        {
            GameObject.DestroyImmediate(go);
            return false;
        }

    }
}