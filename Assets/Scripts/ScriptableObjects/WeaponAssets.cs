using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Weapon Assets")]
    public class WeaponAssets : ScriptableObject
    {
        public GameObject prefab;

        [Header("Sound Clips")]
        public AudioClip shootSound;
        public AudioClip equipSound;
        public AudioClip holsterSound;
        public AudioClip reloadSound;
        public AudioClip aimSound;
    }
}