using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Biome Collection")]
    public class BiomeCollection : ScriptableObject
    {
        public Biome[] biomes;
        public TerrainMaterial[] terrainMaterials;
        public float noiseScale = 0.5f;
        public int seed;

        public TreeType[] allTrees;
        public GrassType[] allDetails;
    }
}