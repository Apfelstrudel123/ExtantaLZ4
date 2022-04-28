using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Grass Type")]
    public class GrassType : ScriptableObject
    {
        public int index;
        [Range(0f, 100f)]
        public float[] probability;
        public GameObject prefab;

        public Color mainColor = new Color(0.2692482f, 0.6603774f, 0f, 1f);
        public Color secondaryColor = new Color(0.2457143f, 0.6037736f, 0f, 1f);
        public bool linkColors;

        public Vector2 minMaxHeight;
        public Vector2 minMaxWidth;

        [Range(0.01f, 0.5f)]
        public float noiseSize = 0.1f;
        public int seed;

        public Vector2 heightRange = new Vector2(0f, 1000f);
        public Vector2 slopeRange = new Vector2(0f, 45f);
        public Vector2 curvatureRange = new Vector2(0f, 1f);
    }
}