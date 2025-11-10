using System.Collections.Generic;
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

    public void Initialize()
    {
        AssignAssetFlippers();
        CreateTrigger();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowAssets();
            GameManager.instance.cameraController.target = new Vector3(rect.center.x, 0, rect.center.y);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            HideAssets();

    }
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
}
