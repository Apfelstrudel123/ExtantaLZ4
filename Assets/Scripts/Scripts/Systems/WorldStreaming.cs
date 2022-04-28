using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Core
{
    public static class WorldStreaming
    {
        //CFG
        private static int zonesPerFrame = 64;
        private static int[] distances;

        private readonly static byte[] loadedLevels = new byte[1024];
        private readonly static byte[] loadingLevels = new byte[1024];
        private readonly static SceneInstance[][] scenes = new SceneInstance[1024][];
        private static int lastChecked = 0;
        private static float loadProgress;
        private static readonly List<AsyncOperationHandle<SceneInstance>> scenesLoading = new List<AsyncOperationHandle<SceneInstance>>();

        public static void Init(int _zonesPerFrame, int[] _distances)
        {
            zonesPerFrame = _zonesPerFrame;
            distances = _distances;

            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = distances[i] * distances[i];
            }
            for (int i = 0; i < 1024; i++)
            {
                scenes[i] = new SceneInstance[3];
            }
        }

        public static void Update()
        {
            for (int i = 0; i < zonesPerFrame; i++)
            {
                CheckZone(lastChecked + i);
            }

            lastChecked += zonesPerFrame;
            if (lastChecked == 1024)
            {
                lastChecked = 0;
            }
        }

        public static void Load(Vector3 pos)
        {
            scenesLoading.Clear();

            for (int i = 0; i < 1024; i++)
            { loadedLevels[i] = 3; loadingLevels[i] = 3; }
            byte lvl;
            string p;
            int x = 0;
            int z = 0;
            AsyncOperationHandle<SceneInstance> s;

            for (int i = 0; i < 1024; i++)
            {
                lvl = GetSceneResult(pos, i);
                loadingLevels[i] = lvl;

                for (byte l = 3; l > lvl; l--)
                {
                    p = "Zone " + x + " " + z + " Level " + (l - 1);
                    SceneLoadedCallback cb = new SceneLoadedCallback { index = i, lvl = l - 1 };
                    s = AddScene(p);
                    scenesLoading.Add(s);
                    s.Completed += cb.OnSceneLoaded;
                }
                z++;
                if (z > 31)
                { z = 0; x++; }
            }

            Time.timeScale = 1;
            GameManager.instance.StartCoroutine(GameManager.instance.GetLoadProgress(2f));
        }
        public static void Teleport(Vector3 pos)
        {
            for (int x = 0; x < 32; x++)
            {
                for (int z = 0; z < 32; z++)
                {
                    CheckZoneTeleport(x, z, pos);
                }
            }
            lastChecked = 0;
        }
        public static void Finish()
        {
            scenesLoading.Clear();
        }

        private static void CheckZone(int index)
        {
            int x = index / 32;
            int z = index - x * 32;
            byte lvl = GetSceneResult(Gameplay.Player.Transform.position, index);

            if (loadingLevels[index] == loadedLevels[index])
            {
                if (lvl > loadedLevels[index])
                {
                    int i = lvl - loadedLevels[index];
                    loadingLevels[index] = lvl;

                    for (int c = 1; c <= i; c++)
                    {
                        RemoveScene(scenes[index][lvl - c]);
                    }
                }
                else if (lvl < loadedLevels[index])
                {
                    int i = loadedLevels[index] - lvl;
                    loadingLevels[index] = lvl;
                    string p;

                    for (int c = 0; c < i; c++)
                    {
                        p = "Zone " + x + " " + z + " Level " + (lvl + c);
                        SceneLoadedCallback cb = new SceneLoadedCallback { index = index, lvl = lvl + c };
                        AddScene(p).Completed += cb.OnSceneLoaded;
                    }
                }
            }
        }

        private static void CheckZoneTeleport(int x, int z, Vector3 pos)
        {
            AsyncOperationHandle<SceneInstance> op;
            int index = x * 32 + z;
            byte lvl = GetSceneResult(pos, index);

            if (lvl > loadedLevels[index])
            {
                int i = lvl - loadedLevels[index];
                loadingLevels[index] = lvl;

                for (int c = 1; c <= i; c++)
                {
                    scenesLoading.Add(RemoveScene(scenes[index][lvl - c]));
                }
            }
            else if (lvl < loadedLevels[index])
            {
                int i = loadedLevels[index] - lvl;
                loadingLevels[index] = lvl;
                string p;

                for (int c = 0; c < i; c++)
                {
                    p = "Zone " + x + " " + z + " Level " + (lvl + c);
                    SceneLoadedCallback cb = new SceneLoadedCallback { index = index, lvl = lvl + c };
                    op = AddScene(p);
                    op.Completed += cb.OnSceneLoaded;
                    scenesLoading.Add(op);
                }
            }
        }

        private static byte GetSceneResult(Vector3 pos, int index)
        {
            byte lvl;
            int x = index / 32;
            int z = index - x * 32;
            Vector3 point = new Vector3((x - 15.5f) * 128f, 0f, (z - 15.5f) * 128f);
            pos.y = 0f;

            float distance = (pos - point).sqrMagnitude;

            if (distance < distances[0])
            {
                lvl = 0;
            }
            else if (distance < distances[1])
            {
                lvl = 1;
            }
            else if (distance < distances[2])
            {
                lvl = 2;
            }
            else
            {
                lvl = 3;
            }
            return lvl;
        }

        private struct SceneLoadedCallback
        {
            public int index;
            public int lvl;
            public void OnSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
            {
                scenes[index][lvl] = obj.Result;
                if (lvl == loadingLevels[index])
                {
                    loadedLevels[index] = loadingLevels[index];
                }
            }
        }

        public static bool Loading()
        {
            for (int i = 0; i < scenesLoading.Count; ++i)
            {
                if (!scenesLoading[i].IsDone)
                { return true; }
            }
            return false;
        }
        public static float GetProgress()
        {
            loadProgress = 0f;
            for (int i = 0; i < scenesLoading.Count; i++)
            {
                if (scenesLoading[i].IsValid())
                {
                    loadProgress += scenesLoading[i].PercentComplete;
                }
                else
                {
                    scenesLoading.RemoveAt(i);
                    i--;
                }
            }
            loadProgress /= scenesLoading.Count;
            return loadProgress;
        }

        private static AsyncOperationHandle<SceneInstance> AddScene(string p)
        {
            return Addressables.LoadSceneAsync(p, LoadSceneMode.Additive);
        }
        private static AsyncOperationHandle<SceneInstance> RemoveScene(SceneInstance scene)
        {
            if (scene.Scene.IsValid())
            {
                return Addressables.UnloadSceneAsync(scene);
            }
            Debug.Log(scene.Scene.name + "Failed Remove");
            AsyncOperationHandle<SceneInstance> empty = new AsyncOperationHandle<SceneInstance>();
            return empty;
        }

        public static void ClearLoading()
        {
            for (int i = 0; i < scenesLoading.Count; i++)
            {
                scenesLoading[i].Task.Dispose();
            }
            scenesLoading.Clear();
        }
        public static void ClearAll()
        {
            for (int i = 0; i < scenesLoading.Count; i++)
            {
                scenesLoading[i].Task.Dispose();
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                for (int u = 0; u < scenes[i].Length; u++)
                {
                    if (scenes[i][u].Scene.IsValid())
                    {
                        RemoveScene(scenes[i][u]);
                    }
                }
            }
            scenesLoading.Clear();
        }
    }
}
