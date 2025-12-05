using UnityEngine;

public abstract class Combatant : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth;
    public int health;
    public int armor;

    public virtual void TakeDamage(int damage)
    {
        int effectiveDamage = damage - armor;
        if (effectiveDamage < 0) effectiveDamage = 0;
        health -= effectiveDamage;
        if (health < 0) health = 0;
    }
    public virtual void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }

    public virtual void Die()
    {

    }
}
