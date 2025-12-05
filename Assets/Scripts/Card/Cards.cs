using UnityEngine;

public class Cards : MonoBehaviour
{
    public static Cards instance;
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [Header("Prefabs")]
    public CardPresenter baseCardPrefab;

    [Header("ScriptableObjects")]
    public MovementCard movementCard;
    public InvestigateCard investigateCard;
}
