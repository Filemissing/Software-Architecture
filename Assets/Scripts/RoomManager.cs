using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public RectInt rect;

    [Header("Asset references")]
    public Transform wallParent;
    public Transform floorParent;
    public Transform doorParent;
    public Transform decorationParent;

    public List<GameObject> doors = new();
    public List<GameObject> decorations = new();

    public List<AssetFlipper> assetFlippers = new();

    [Header("Neighbours")]
    public List<RoomManager> neighbours = new();

    [Header("Combat")]
    public bool hasCombat;
    public CombatEncounter encounter;

    // setup
    public void Initialize()
    {
        CreateTrigger();
        EventBus<RoomEnter>.onEvent += OnRoomEnter;
    }
    void CreateTrigger()
    {
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        boxCollider.center = new Vector3(rect.center.x, 2.5f, rect.center.y);
        boxCollider.size = new Vector3(rect.width, 5, rect.height);
    }

    void OnRoomEnter(RoomEnter roomEnter)
    {
        if (roomEnter.to == this)
        {
            ShowAssets();

            GameManager.instance.cameraController.SetTarget(new Vector3(rect.center.x, 0, rect.center.y));
            GameManager.instance.currentRoom = this;

            Cardbar.instance.Clear();
            for (int i = 0; i < doors.Count; i++)
            {
                MovementCard cardInstance = MovementCard.Create(i, doors[i].transform.GetChild(0).gameObject);
                CardPresenter card = CardPresenter.Create(cardInstance);
                Cardbar.instance.AddCard(card);
            }

            if (decorations.Count > 0) 
            {
                int investigateCardCount = Random.Range(1, decorations.Count);
                for (int i = 0; i < investigateCardCount; i++)
                {
                    CardPresenter card = CardPresenter.Create(Cards.instance.investigateCard);
                    Cardbar.instance.AddCard(card);
                }
            }
        }
        else if (roomEnter.from == this)
        {
            HideAssets(roomEnter.to);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            EventBus<RoomEnter>.Publish(new RoomEnter(GameManager.instance.currentRoom, this));
    }

    // helper methods
    public void ShowAssets(bool instant = false)
    {
        foreach (AssetFlipper flipper in assetFlippers)
            if (instant)
                flipper.ShowInstant();
            else
                flipper.Show();

        foreach (GameObject decoration in decorations) 
            decoration.SetActive(true);
    }
    public void HideAssets(bool instant = false)
    {
        foreach (AssetFlipper flipper in assetFlippers)
            if (instant)
                flipper.HideInstant();
            else
                flipper.Hide();

        foreach (GameObject decoration in decorations)
            decoration.SetActive(false);
    }
    public void HideAssets(RoomManager overLappingRoom, bool instant = false)
    {
        // avoid flipping assets that are shared with the specific room
        AssetFlipper[] objectsToHide = assetFlippers.Where(flipper => !overLappingRoom.assetFlippers.Contains(flipper)).ToArray();

        foreach (AssetFlipper flipper in objectsToHide)
            if (instant)
                flipper.HideInstant();
            else
                flipper.Hide();

        foreach (GameObject decoration in decorations)
            decoration.SetActive(false);
    }
}

public struct RoomEnter
{
    public RoomEnter(RoomManager from, RoomManager to)
    {
        this.from = from;
        this.to = to;
    }
    public RoomManager from;
    public RoomManager to;
}
