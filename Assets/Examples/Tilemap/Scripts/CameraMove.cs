using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Tilemaps.Example
{
    public class CameraMove : MonoBehaviour
    {

        public float speed = 10;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            Vector3 motion = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            Vector3 pos = transform.position;
            pos += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * (motion * speed * Time.deltaTime);
            transform.position = pos;
        }
    }
}