using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tree Type")]
    public class TreeType : ScriptableObject
    {
        public int index;
        [Range(0f, 100f)]
        public float[] probability;
        public GameObject prefab;

        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        public float sinkAmount = 0f;
     
        //public int seed;

        public Vector2 heightRange = new Vector2(0f, 1000f);
        public Vector2 slopeRange = new Vector2(0f, 60f);
        public Vector2 curvatureRange = new Vector2(0f, 1f);
    }
}