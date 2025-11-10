using DG.Tweening;
using UnityEngine;

public class AssetFlipper : MonoBehaviour
{
    public Vector3 originalRotation;
    public Vector3 flippedRotation;
    bool isEnabled = true; // debugging only

    private void Awake()
    {
        originalRotation = transform.rotation.eulerAngles;
    }

    public void Hide()
    {
        if (isEnabled)
        {
            transform.DOKill();
            transform.DORotate(flippedRotation, 1).SetEase(Ease.InOutSine);
        }
    }
    public void HideInstant()
    {
        if (isEnabled)
            transform.rotation = Quaternion.Euler(flippedRotation);
    }

    public void Show()
    {
        if (isEnabled)
        {
            transform.DOKill();
            transform.DORotate(originalRotation, 1).SetEase(Ease.InOutSine);
        }
    }
    public void ShowInstant()
    {
        if (isEnabled)
            transform.rotation = Quaternion.Euler(originalRotation);
    }
}
