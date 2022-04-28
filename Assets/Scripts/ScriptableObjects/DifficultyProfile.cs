using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Difficulty Profile")]
    public class DifficultyProfile : ScriptableObject
    {
        public float abilityProgressMultiplier;
    }
}