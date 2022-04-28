using UnityEngine;
using Gameplay.Abilities;
namespace AI
{
    public class AIHitbox : MonoBehaviour
    {
        [SerializeField] private int damageMultiplier = 1;
        [HideInInspector] public NPC parent;

        public void TakeDamage(float amt)
        {
            if (parent.Type() == AIType.Soldier)
            {
                AbilitySystem.UpdateValue(AbilityCategory.Shooting, damageMultiplier);
            }
            if (parent != null)
            {
                parent.Health().AddHealth(-amt * damageMultiplier, ObjectHealth.DamageType.Bullet);
            }
            else
            {
                Debug.LogWarning("AIHitbox parent not assigned");
            }
        }
    }
}