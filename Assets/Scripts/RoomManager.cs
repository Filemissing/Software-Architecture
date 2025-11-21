using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public RectInt rect;

    [Header("Asset references")]
    public Transform wallParent;
    public List<GameObject> walls = new();

    public Transform floorParent;
    public List<GameObject> floors = new();

    public Transform doorParent;
    public List<GameObject> doors = new();

    List<AssetFlipper> assetFlippers = new();

    [Header("Neighbours")]
    public List<RoomManager> neighbours = new();

    // setup
    public void Initialize()
    {
        AssignAssetFlippers();
        CreateTrigger();
        EventBus<RoomEnter>.onEvent += OnRoomEnter;
    }
    void AssignAssetFlippers()
    {
        assetFlippers.Clear();

        foreach (GameObject wall in walls)
            assetFlippers.AddRange(wall.GetComponentsInChildren<AssetFlipper>());

        foreach (GameObject floor in floors)
            assetFlippers.AddRange(floor.GetComponentsInChildren<AssetFlipper>());
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
            GameManager.instance.cameraController.target = new Vector3(rect.center.x, 0, rect.center.y);
            GameManager.instance.currentRoom = this;
            Cardbar.instance.Clear();
            for (int i = 0; i < doors.Count; i++)
            {
                MovementCard cardInstance = MovementCard.Create(i, doors[i].transform.GetChild(0).gameObject);
                CardPresenter card = Instantiate(Cards.instance.baseCardPrefab);
                card.card = cardInstance;
                Cardbar.instance.AddCard(card);
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
    }
    public void HideAssets(bool instant = false)
    {
        foreach (AssetFlipper flipper in assetFlippers)
            if (instant)
                flipper.HideInstant();
            else
                flipper.Hide();
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
