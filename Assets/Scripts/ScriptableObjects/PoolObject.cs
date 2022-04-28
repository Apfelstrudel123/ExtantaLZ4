using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Pool Object")]
    public class PoolObject : ScriptableObject
    {
        public string[] prefabs;
        public int createAtStart = 0;
        public int limit = 10;
    }
}