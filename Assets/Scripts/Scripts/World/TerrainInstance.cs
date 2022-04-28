using UnityEngine;
using ScriptableObjects;
using JBooth.MicroSplat;
namespace GameWorld
{
    public class TerrainInstance : MonoBehaviour
    {
        public BiomeType[] biomes = new BiomeType[] { BiomeType.Rocks };

        private MicroSplatTerrain microsplat;

        private void Start()
        {
            if (TryGetComponent<MicroSplatTerrain>(out microsplat))
            {
                //microsplat.tintMapOverride = Enviroment.defaultTerrainTint;
            }
        }
    }
}