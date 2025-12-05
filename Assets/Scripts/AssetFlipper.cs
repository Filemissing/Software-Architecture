using DG.Tweening;
using UnityEngine;

public class AssetFlipper : MonoBehaviour
{
    MeshRenderer meshRenderer;

    [SerializeField] Vector3 originalRotation;
    [SerializeField] Vector3 flippedRotation;
    bool isEnabled = true; // debugging only

    private void Awake()
    {
        originalRotation = transform.rotation.eulerAngles;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Hide()
    {
        if (isEnabled)
        {
            transform.DOKill();
            transform.DORotate(flippedRotation, 1).SetEase(Ease.OutBack);

            Invoke("DisableMeshRenderer", 1);
        }
    }
    void DisableMeshRenderer()
    {
        meshRenderer.enabled = false;
    }

    public void HideInstant()
    {
        if (isEnabled)
        {
            transform.rotation = Quaternion.Euler(flippedRotation);
            meshRenderer.enabled = false;
        }
    }

    public void Show()
    {
        if (isEnabled)
        {
            transform.DOKill();
            transform.DORotate(originalRotation, 1).SetEase(Ease.OutBack);
            meshRenderer.enabled = true;
        }
    }
    public void ShowInstant()
    {
        if (isEnabled)
        {
            transform.rotation = Quaternion.Euler(originalRotation);
            meshRenderer.enabled = true;
        }
    }
}
