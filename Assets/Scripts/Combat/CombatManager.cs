using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    [Header("References")]
    [SerializeField] EnemyCombat enemyPresenter;

    [Header("State")]
    [SerializeField] CombatEncounter encounter;

    [Header("Combatants")]
    [SerializeField] PlayerCombat player;
    [SerializeField] List<EnemyCombat> enemies;

    public void StartCombat()
    {

    }

    public void LoadEncounter()
    {
        enemies = new List<EnemyCombat>();
        foreach (var enemyData in encounter.enemies)
        {
            EnemyCombat enemyCombat = Instantiate(enemyPresenter, transform.position, Quaternion.identity);
            enemyCombat.Initialize(enemyData);
            enemies.Add(enemyCombat);
        }
    }
}
