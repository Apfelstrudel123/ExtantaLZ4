using UnityEngine;
namespace Gameplay
{
    public class WeaponPickup : MonoBehaviour
    {
        public string weaponKey;

        private void PickUp()
        {
            Combat.Weapons.PickUpWeapon(weaponKey);
        }
    }
}