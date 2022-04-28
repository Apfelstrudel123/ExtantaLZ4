using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Stairs Profile")]
    public class StairsProfile : ScriptableObject
    {
        public enum WallType
        {
            NoWall = 0,
            OneWall = 1,
            TwoWall = 2,
        }
        public StairsElement[] elements;
        public WallType wallType;
        public GameObject wall;
    }

    [CreateAssetMenu(menuName = "ScriptableObjects/Stairs Element")]
    public class StairsElement : ScriptableObject
    {
        public GameObject model;
        public float depth;
        public float height;
        public Quaternion rotation;
    }
}