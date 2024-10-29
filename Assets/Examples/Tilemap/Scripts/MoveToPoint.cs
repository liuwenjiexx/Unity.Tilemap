using System.Collections;
using System.Collections.Generic;
using Unity.Tilemaps;
using UnityEngine;
using UnityEngine.AI;

namespace UnityEngine.Tilemaps.Example
{

    public class MoveToPoint : MonoBehaviour
    {
        TilemapCreator tilemapCreator;
        NavMeshAgent agent;
        Vector3 targetPos;
        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            tilemapCreator = TilemapCreator.all[0];
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (tilemapCreator.RayToWorldPosition(ray, out Vector3 pos))
                {
                    targetPos = pos;
                    agent.SetDestination(pos);
                }
            }
            Debug.DrawRay(targetPos, Vector3.up);
        }

    }
}
