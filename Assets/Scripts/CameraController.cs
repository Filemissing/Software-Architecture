using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 target;
    public Vector3 offset;
    public float moveDuration;

    private void Update()
    {
        if (target == null) return;

        transform.DOMove(target + offset, moveDuration).SetEase(Ease.InOutSine);
    }
}
