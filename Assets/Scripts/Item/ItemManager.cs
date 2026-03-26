using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private Collider2D itemCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private bool isCollected;
    private bool isCommitted;

    private void Awake()
    {
        if (itemCollider == null)
        {
            itemCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void Collect()
    {
        if (isCollected) return;

        isCollected = true;
        isCommitted = false;

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Collect");
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Commit()
    {
        if (!isCollected) return;

        isCommitted = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        gameObject.SetActive(false);
    }

    public void ResetUncommitted()
    {
        if (!isCollected) return;
        if (isCommitted) return;

        isCollected = false;

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Animator‚ª‚ ‚é‚È‚ç•K—v‚É‰‚¶‚Ä‰Šúó‘Ô‚Ö
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    // Animation Event
    public void HideVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
}
