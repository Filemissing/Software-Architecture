using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Vector3 target;
    [SerializeField] Vector3 offset;
    [SerializeField] float moveDuration;

    [SerializeField] AnimationCurve XcursorInfluenceFalloff;
    [SerializeField] AnimationCurve YcursorInfluenceFalloff;

    private void Update()
    {
        if (target == null) return;

        Vector2 cursorUV = Input.mousePosition / new Vector2(Screen.width, Screen.height);

        Vector3 lookDirection = target - transform.position;
        lookDirection += transform.right * XcursorInfluenceFalloff.Evaluate(cursorUV.x);
        lookDirection += transform.up * YcursorInfluenceFalloff.Evaluate(cursorUV.y);

        Vector3 lookTarget = transform.position + lookDirection;

        transform.DOLookAt(lookTarget, moveDuration).SetEase(Ease.InOutSine);
        transform.DOMove(target + offset, moveDuration).SetEase(Ease.InOutSine);
    }

    public void SetTarget(Vector3 position) => target = position;
}
