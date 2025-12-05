using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    [SerializeField] private Sprite walkSprite;
    [SerializeField] private Sprite idleSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (spriteRenderer == null || playerController == null) return;

        if (playerController.moveDir.magnitude > 0.001f)
        {
            spriteRenderer.sprite = walkSprite;
            if (playerController.moveDir.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
        else
        {
            spriteRenderer.sprite = idleSprite;
        }
    }
}
