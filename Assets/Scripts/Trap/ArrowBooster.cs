using UnityEngine;
using System.Collections;

public class ArrowBooster : MonoBehaviour
{
    [Header("Launch")]
    [SerializeField] private float launchPower = 18f;
    [SerializeField] private bool useTransformRight = true;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 1.5f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Animator animator;

    [SerializeField] private float hitAnimationTime = 0.15f;

    private bool isActive = true;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        triggerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        PlayerManager player = other.GetComponent<PlayerManager>();
        if (player == null) return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Activate(player, rb);
    }

    private void Activate(PlayerManager player, Rigidbody2D rb)
    {
        isActive = false;

        Vector2 launchDirection = useTransformRight ? transform.right : transform.up;

        Vector2 launchVelocity = launchDirection.normalized * launchPower;

        if (Mathf.Abs(launchVelocity.y) < 0.1f)
        {
            launchVelocity.y = 3f;
        }

        player.ApplyArrowBoost(launchVelocity);

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        yield return new WaitForSeconds(hitAnimationTime);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(respawnTime);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        isActive = true;

        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 dir = useTransformRight ? transform.right : transform.up;

        Gizmos.DrawLine(transform.position, transform.position + dir.normalized * 1.5f);
    }
#endif
}
