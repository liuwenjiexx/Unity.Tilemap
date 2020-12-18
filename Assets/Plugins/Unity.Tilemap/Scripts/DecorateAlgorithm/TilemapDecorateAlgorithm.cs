using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.Tilemaps
{
    [Serializable]
    public class TilemapDecorateAlgorithm
    {
        public virtual void Generate(TilemapCreator creator, int layer, TilemapData map, DecorateConfig decorateConfig)
        {

        }
        public void CreateDecorateObject(TilemapCreator creator, DecorateConfig decorateConfig, int layer, int x, int y, TileFlags tileFlags)
        {
            Vector3 offset;
            Vector3 rotation;
            float scale = 1f;


            offset = decorateConfig.offset;

            if (decorateConfig.useRandomOffset)
            {
                offset += decorateConfig.minRandomOffset.RandomVector3(decorateConfig.maxRandomOffset);
            }

            rotation = decorateConfig.offsetRotation;
            float tileRotAngle = tileFlags.GetOffsetAngle();
            Quaternion tileRot = Quaternion.Euler(new Vector3(0f, tileRotAngle, 0f));

            if (decorateConfig.useTileRotation)
            {
                rotation.y += tileRotAngle;
            }

            if (decorateConfig.useRandomRotation)
            {
                rotation += decorateConfig.minRandomRotation.RandomVector3(decorateConfig.maxRandomRotation);
            }

            if (decorateConfig.useRandomScale)
            {
                scale = scale * Random.Range(decorateConfig.minRandomScale, decorateConfig.maxRandomScale);
            }
            Transform parent = creator.GetDecoratesRoot(layer);
            Vector3 pos = creator.GridToWorldPosition(x, y) + tileRot * (offset * creator.Tilemap.scale);


            if (Application.isPlaying && (!creator || creator.InstantiateGameObject == null))
            {
                GameObject go = GameObject.Instantiate(decorateConfig.prefab);
                go.transform.parent = parent;
                go.transform.localPosition = pos;
                go.transform.localEulerAngles = rotation;
                go.transform.localScale = go.transform.localScale * scale;
            }
            else
            {
                TilemapInstantiateData instantiateData = new TilemapInstantiateData()
                {
                    x = x,
                    y = y,
                    parent = parent,
                    prefab = decorateConfig.prefab,
                    position = pos,
                    rotation = rotation,
                    scale = new Vector3(scale, scale, scale),
                };

                if (Application.isPlaying)
                {
                    creator.InstantiateGameObject(instantiateData);
                }
                else
                {
                    TilemapCreator.EditorInstantiateGameObject(instantiateData);
                }
            }



        }

        //protected virtual GameObject CreateDecorateObject()
        //{

        //}

    }
}