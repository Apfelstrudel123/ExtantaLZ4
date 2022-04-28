using UnityEngine;
using UnityEngine.Events;
public class ObjectHealth : MonoBehaviour
{
    public enum DamageType
    {
        Bullet,
        Fire,
        Fall,
        Explosion,
        Script,
    }   

    public float maxHealth;
    public float currentHealth;

    [SerializeField] private UnityEvent onDie;
    public UnityAction<DamageType> onDamage;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void AddHealth(float amt, DamageType origin)
    {
        currentHealth += amt;
        if(currentHealth > maxHealth)
        { currentHealth = maxHealth; }

        if(currentHealth <= 0 && onDie != null)
        { onDie.Invoke(); }
        else if (amt != 0 && onDamage != null)
        { onDamage.Invoke(origin); }
    }
    public void SubtractHealth(float amt, DamageType origin)
    {
        currentHealth -= amt;
        if (currentHealth > maxHealth)
        { currentHealth = maxHealth; }

        if (currentHealth <= 0 && onDie != null)
        { onDie.Invoke(); }
        else if (amt != 0 && onDamage != null)
        { onDamage.Invoke(origin); }
    }
}