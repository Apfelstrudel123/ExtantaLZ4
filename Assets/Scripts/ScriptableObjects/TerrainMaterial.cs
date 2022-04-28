using UnityEngine;
namespace ScriptableObjects
{
    public enum BiomeType
    {
        Rocks = 0,
        Field = 1,
        Forest = 2,
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/Terrain Material")]
    public class TerrainMaterial : ScriptableObject
    {
        public float seed = 0f;
        public int LODLayer;
        public Color LODColor = Color.green;
        public Color secondaryLODColor = Color.green;

        public bool useNoise = true;
        public bool procedural = true;

        public BiomeType[] biomes = new BiomeType[] { BiomeType.Rocks };

        public int minHeight = 0;
        public int maxHeight = 600;

        public float minSlope = 0f;
        public float maxSlope = 30f;

        public bool trees = true;
        public bool gras = true;
    }
}