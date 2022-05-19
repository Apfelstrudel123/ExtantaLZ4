using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ScriptableObjects;
namespace Core
{
    public static class ObjectPool
    {
        private static PoolObject[] data;
        private static GameObject[] prefabs;
        private static List<GameObject>[] objects;
        private static int[] lastRecycled;
        private static readonly Dictionary<string, int> identifiers = new Dictionary<string, int>();

        public static void Init()
        {
            Quaternion q = Quaternion.Euler(0f, 0f, 0f);
            data = Resources.LoadAll<PoolObject>("PoolData/");

            prefabs = new GameObject[data.Length];
            objects = new List<GameObject>[data.Length];
            lastRecycled = new int[data.Length];
            identifiers.Clear();
            for (int i = 0; i < data.Length; i++)
            {
                objects[i] = new List<GameObject>();
                identifiers.Add(data[i].name, i);
                for (int c = 0; c < data[i].createAtStart; c++)
                {
                    Request(data[i].name, Vector3.zero, q, InitObject);
                }
            }
        }
        private static void InitObject(GameObject obj)
        {
            obj.SendMessage("Hide", SendMessageOptions.DontRequireReceiver);
        }

        public static void TidyUp()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                foreach (GameObject g in objects[i])
                {
                    g.SendMessage("Hide", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public static void GetPoolContent(string key, out GameObject[] g)
        {
            if (identifiers.TryGetValue(key, out int i))
            {
                g = objects[i].ToArray();
            }
            else
            {
                Debug.Log("Invalid Key");
                g = null;
            }
        }

        public static void Request(string id, Vector3 position, Quaternion rotation, System.Action<GameObject> onLoaded)
        {
            if (!identifiers.TryGetValue(id, out int type))
            {
                Debug.Log("Invalid Key `" + id + "`");
            }

            foreach (GameObject obj in objects[type])
            {
                if (obj.activeInHierarchy == false)
                {
                    obj.SetActive(true);
                    obj.transform.SetPositionAndRotation(position, rotation);

                    if (onLoaded != null)
                    {
                        onLoaded.Invoke(obj);
                    }
                    return;
                }
            }

            if (objects[type].Count >= data[type].limit && data[type].limit > 0)
            {
                GameObject obj = objects[type][lastRecycled[type]];
                lastRecycled[type]++;
                if (lastRecycled[type] >= data[type].limit)
                { lastRecycled[type] = 0; }

                if (obj.activeInHierarchy)
                { obj.SendMessage("OnHideEarly"); }
                obj.SetActive(true);
                obj.transform.SetPositionAndRotation(position, rotation);

                if (onLoaded != null)
                {
                    onLoaded.Invoke(obj);
                }
                return;
            }

            int i = 0;
            if (data[type].prefabs.Length > 1)
            {
                i = Random.Range(0, data[type].prefabs.Length);
            }

            if (prefabs[type] != null)
            {
                GameObject g = Object.Instantiate(prefabs[type], position, rotation);

                objects[type].Add(g);

                if (onLoaded != null)
                {
                    onLoaded.Invoke(g);
                }
                return;
            }
            WaitForLoad(data[type].prefabs[i], type, position, rotation, onLoaded);
        }

        public static async void WaitForLoad(string key, int type, Vector3 position, Quaternion rotation, System.Action<GameObject> onLoaded)
        {
            GameObject go = await Addressables.InstantiateAsync(key, position, rotation).Task;

            prefabs[type] = go;
            objects[type].Add(go);

            if (onLoaded != null)
            {
                onLoaded.Invoke(go);
            }
        }
    }
}
