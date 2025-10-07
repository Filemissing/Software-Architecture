using UnityEngine;

public class ProximityFlipper : MonoBehaviour
{
    Vector3 originalRotation;
    public Vector3 flippedRotation;
    bool isEnabled = true;

    private void Start()
    {
        if (isEnabled)
        {
            originalRotation = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(flippedRotation); 
        }
    }

    private void Update()
    {
        if (isEnabled)
        {
            float distance = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);
            float lerpValue = Mathf.Pow(distance / 10f, 2) - 2;
            lerpValue = Mathf.Clamp01(lerpValue);
            transform.eulerAngles = Vector3.Lerp(originalRotation, flippedRotation, lerpValue); 
        }
    }
}
