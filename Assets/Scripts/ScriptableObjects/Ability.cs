using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Ability")]
    public class Ability : ScriptableObject
    {
        public string _name;
        public string info;
        public int cost;
    }
}
