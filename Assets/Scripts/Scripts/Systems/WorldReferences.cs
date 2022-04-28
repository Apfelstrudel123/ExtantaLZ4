using UnityEngine;
namespace GameWorld
{
    public class WorldReferences : MonoBehaviour
    {
        public static WorldReferences instance;

        private void Awake()
        {
            instance = this;
            Core.GameManager.instance.locations = locations;
            Enviroment.instance.terrains = terrains;
            Enviroment.showTrees = terrains.GetChild(0).GetComponent<Terrain>().drawTreesAndFoliage;
        }

        [Header("References")]
        public Transform terrains;
        public GameObject locations;
    }
}