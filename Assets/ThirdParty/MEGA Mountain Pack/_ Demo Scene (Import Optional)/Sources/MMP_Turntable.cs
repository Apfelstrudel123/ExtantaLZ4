using System.Collections.Generic;
using UnityEngine;

namespace MMP
{
    public class MMP_Turntable : MonoBehaviour
    {

        public float rotationSpeed = 1;
        public float displayDuration = 1;
        public Transform camT;
        List<GameObject> mountains = new List<GameObject>();
        float time;
        int index;

        void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                mountains.Add(child);
                if (i != 0) child.SetActive(false);
            }
        }

        void Update()
        {
            camT.Rotate(0, Time.deltaTime * rotationSpeed, 0);
            time += Time.deltaTime;
            if (time > displayDuration)
            {
                mountains[index].SetActive(false);
                index++; index %= mountains.Count;
                mountains[index].SetActive(true);
                time = 0;
            }

            float scale = transform.localScale.y;
            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                scale += Time.deltaTime * 0.5f;
            }
            if (Input.GetKey(KeyCode.KeypadMinus))
            {
                scale -= Time.deltaTime * 0.5f;
            }

            scale += Input.mouseScrollDelta.y * 0.05f;

            scale = Mathf.Clamp(scale, 0.2f, 1.3f);
            transform.localScale = new Vector3(1, scale, 1);
        }
    }
}
