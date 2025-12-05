using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu]
public class InvestigateCard : Card
{
    [SerializeField] LayerMask layerMask;
    public override bool Play()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("ToDo: Give player random loot");
            return true;
        }
        return false;
    }

    public override void OnDrag(PointerEventData eventData, CardPresenter presenter, bool isInPlayArea)
    {
        if (isInPlayArea && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            presenter.background.color = Color.yellow;
        }
        else presenter.background.color = Color.white;
    }
}
