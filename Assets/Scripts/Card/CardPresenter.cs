using UnityEngine;
using UnityEngine.EventSystems;

public class CardPresenter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static float activationHeightPercentage = .3f;

    public Card card;

    bool isDragging = false;
    Transform oldParent;
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        oldParent = transform.parent;
        transform.parent = transform.parent = transform.root;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        transform.position = eventData.position;

        // add border effect when in play area
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (eventData.position.y >= Screen.height * activationHeightPercentage)
        {
            card.Play();
            Destroy(gameObject);
        }
        else
        {
            transform.parent = oldParent;
        }
    }
}
