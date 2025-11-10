using System.Collections.Generic;
using UnityEngine;

public class Cardbar : MonoBehaviour
{
    List<CardPresenter> cards = new List<CardPresenter>();

    [SerializeField] Vector3 pivotPosition;
    [SerializeField] float angleBetween;
    
    public void AddCard(CardPresenter card)
    {
        cards.Add(card);
        int index = cards.FindIndex(c => c == card);

        Transform cardPivot = card.transform.parent;
        cardPivot.parent = transform;
        cardPivot.localPosition = pivotPosition;
        cardPivot.rotation = Quaternion.Euler(0, 0, angleBetween * index);
    }
}
