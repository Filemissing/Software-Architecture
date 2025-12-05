using UnityEngine;

[CreateAssetMenu()]
public class EnemyData : ScriptableObject
{
    public Sprite sprite;
    public int maxHealth;
    public EnemyAction[] actions;
}
