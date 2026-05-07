using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private Vector2 rayOffset = new Vector2(0f, 0.1f);

    [Header("Appearance")]
    [SerializeField] private Vector3 nearScale = new Vector3(1f, 0.35f, 1f);
    [SerializeField] private Vector3 farScale = new Vector3(0.4f, 0.15f, 1f);
    [SerializeField] private float nearAlpha = 0.45f;
    [SerializeField] private float farAlpha = 0.08f;

    [Header("Position")]
    [SerializeField] private float groundOffsetY = 0.03f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            SetVisible(false);
            return;
        }

        Vector2 origin = (Vector2)player.position + rayOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxDistance, groundLayer);

        if (!hit.collider)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        transform.position = new Vector3(player.position.x, hit.point.y + groundOffsetY, transform.position.z);

        float distanceRate = Mathf.Clamp01(hit.distance / maxDistance);

        transform.localScale = Vector3.Lerp(nearScale, farScale, distanceRate);

        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(nearAlpha, farAlpha, distanceRate);
        spriteRenderer.color = color;
    }

    private void SetVisible(bool visible)
    {
        if (spriteRenderer.enabled != visible)
        {
            spriteRenderer.enabled = visible;
        }
    }
}
