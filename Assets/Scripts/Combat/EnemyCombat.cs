using UnityEngine;

public class EnemyCombat : Combatant
{
    [SerializeField] EnemyData data;

    public void Initialize(EnemyData data)
    {
        this.data = data;
        maxHealth = data.maxHealth;
        health = maxHealth;

    }
}
