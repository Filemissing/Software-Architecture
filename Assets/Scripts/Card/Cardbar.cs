using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Cardbar : MonoBehaviour
{
    public static Cardbar instance;
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
        }
    }

    public List<CardPresenter> cards = new List<CardPresenter>();

    [SerializeField] Vector3 pivotPosition;
    [SerializeField] float angleBetween;

    RectTransform rectTransform;
    public void Update()
    {
        if(!rectTransform) rectTransform = transform as RectTransform;

        float startingAngle = -(angleBetween * (cards.Count - 1) / 2);
        for (int i = 0; i < cards.Count; i++)
        {
            CardPresenter card = cards[i];

            RectTransform cardPivot = card.transform as RectTransform;

            cardPivot.pivot = new Vector2(0.5f, -Vector2.Distance(pivotPosition, rectTransform.pivot) / card.background.rectTransform.rect.height);
            cardPivot.localRotation = Quaternion.Euler(0, 0, startingAngle + angleBetween * i);
        }
    }

    public void AddCard(CardPresenter card)
    {
        if (cards.Contains(card)) return;
        cards.Add(card);
        card.transform.parent = transform;
        card.transform.localPosition = pivotPosition;
    }
    public void RemoveCard(CardPresenter card, bool delete = false)
    {
        if (!cards.Contains(card)) return;
        cards.Remove(card);

        if(delete)
            Destroy(card.gameObject);
    }
    public void Clear()
    {
        cards.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
