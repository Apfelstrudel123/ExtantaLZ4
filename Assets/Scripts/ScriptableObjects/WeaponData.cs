using UnityEngine;
namespace ScriptableObjects
{
    public class WeaponData : ScriptableObject
    {
        [HideInInspector] public int weaponID;
        public bool automatic;
        public bool sidearm;

        public float fovSpeed = 50f;
        public float aimFov = 45f;

        [Header("Accuracy")]
        public float xSpread;
        public float ySpread;
        public float xRecoil;
        public float yRecoil;

        public Vector3 casingSpawnPoint;
        public Quaternion casingSpawnRot;
        public Vector3 muzzlePos;

        public float fireRate;
        public int magSize;
        public int damage;

        public float bulletForce = 400.0f;

        public float mass = 20f;

        [Header("Durations")]
        public float equipTime = 0.25f;
        public float holsterTime = 0.25f;
        public float reloadTime;
    }
}