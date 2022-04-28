using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Biome")]
    public class Biome : ScriptableObject
    {
        public int index;
        public TreeType[] trees;
        public float treeProbability;
        public float treeDistance;

        public GrassType[] details;
        public float grassProbability;
    }
}