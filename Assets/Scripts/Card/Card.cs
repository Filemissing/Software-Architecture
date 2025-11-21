using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Card : ScriptableObject
{
    public Sprite background;
    public Sprite descriptionBackground;
    public Sprite titleBackground;

    public Sprite art;

    public string title;
    public string description;

    public abstract bool Play();

    public virtual void OnStartDrag(PointerEventData eventData, CardPresenter presenter) { }
    public virtual void OnDrag(PointerEventData eventData, CardPresenter presenter) { }
    public virtual void OnEndDrag(PointerEventData eventData, CardPresenter presenter) { }
    public virtual void OnPointerEnter(PointerEventData eventData, CardPresenter presenter) { }
    public virtual void OnPointerExit(PointerEventData eventData, CardPresenter presenter) { }
}
