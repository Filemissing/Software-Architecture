using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu]
public class MovementCard : Card
{
    public int exitNumber;
    public GameObject exitOutline;

    public override bool Play()
    {
        Vector2 center = GameManager.instance.currentRoom.neighbours[exitNumber].rect.center;
        Vector3 target = new Vector3(center.x, 0, center.y);
        GameManager.instance.player.Move(target);
        return true;
    }

    public override void OnDrag(PointerEventData eventData, CardPresenter presenter, bool isInPlayArea)
    {
        exitOutline.SetActive(true);

        // add border effect when in play area
        if (isInPlayArea)
        {
            presenter.background.color = Color.yellow;
        }
        else
        {
            presenter.background.color = Color.white;
        }
    }
    public override void OnEndDrag(PointerEventData eventData, CardPresenter presenter)
    {
        base.OnEndDrag(eventData, presenter);
        exitOutline.SetActive(false);
    }

    public static MovementCard Create(int exitNumber, GameObject exitOutline)
    {
        MovementCard card = Instantiate(Cards.instance.movementCard);
        card.exitNumber = exitNumber;
        card.exitOutline = exitOutline;
        card.title = "Move";
        card.description = "exit " + (exitNumber + 1);
        return card;
    }
}
