using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// where namespace?

public class CardPresenter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    static float ACTIVATION_HEIGHT_PERCENTAGE = .3f; // Should be all caps due to being a constant

    public Card card;

    [Header("References")]
    public Image background; // Don't have to be public
    public Image descriptionBackground;
    public Image titleBackground;

    public Image art;

    public TMP_Text title;
    public TMP_Text description;

    Color originalBackgroundColor;
    private void Start()
    {
        originalBackgroundColor = background.color;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if(card.background) background.sprite = card.background;
        if(card.descriptionBackground) descriptionBackground.sprite = card.descriptionBackground;
        if(card.titleBackground) titleBackground.sprite = card.titleBackground;
        if(card.art) art.sprite = card.art;

        title.text = card.title;
        description.text = card.description;
    }

    // dragging logic
    bool isDragging = false;
    Transform oldParent;
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        oldParent = transform.parent;
        transform.parent = transform.parent = transform.root;

        Cardbar.instance.RemoveCard(this);

        // reset display
        RectTransform rectTransform = transform as RectTransform;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.DORotate(Vector2.zero, .2f).SetEase(Ease.OutSine);

        card.OnStartDrag(eventData, this);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        card.OnDrag(eventData, this);

        transform.position = eventData.position;

        // add border effect when in play area
        if(eventData.position.y > Screen.height * ACTIVATION_HEIGHT_PERCENTAGE)
        {
            background.color = Color.white;
        }
        else
        {
            background.color = originalBackgroundColor;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        card.OnEndDrag(eventData, this);

        if (eventData.position.y >= Screen.height * ACTIVATION_HEIGHT_PERCENTAGE)
        {
            if (card.Play())
            {
                Destroy(gameObject);
                return;
            }
        }

        transform.parent = oldParent;
        Cardbar.instance.AddCard(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        card.OnPointerEnter(eventData, this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        card.OnPointerExit(eventData, this);
    }
}
