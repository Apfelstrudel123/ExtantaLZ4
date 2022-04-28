using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AI Settings")]
    public class AISettings : ScriptableObject
    {
        [Header("References")]
        public GameObject neutralBase;

        [Space()]
        [Header("Type Settings")]
        [Tooltip("0 - Soldier, 1 - Rebel, 2 - Pedestrian, 3 - Peaceful Animal, 4 - Aggressive Animal")]
        public AITypeSettings[] typeSettings;

        [Space()]
        [Header("Combat Settings")]
        public LayerMask shootMask = new LayerMask();
        public float bulletDistance = 120f;
        public AudioClip shootClip = null;
        public AudioClip reloadClip = null;
    }

    [System.Serializable]
    public class AITypeSettings
    {
        public int removeDistance = 70;
        public int deadRemoveDistance = 50;
        public int maxSpawnedLimit = 20;
        public GameObject[] npcPrefabs;
    }
}