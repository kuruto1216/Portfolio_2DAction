using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;
    [SerializeField] Animator animator;

    bool activated;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CheckpointManager.Instance.SetCheckpoint(respawnPoint.position);

        if (activated) return;

        activated = true;

        animator.SetTrigger("Raise");
        animator.SetBool("Activated", true);
    }
}
