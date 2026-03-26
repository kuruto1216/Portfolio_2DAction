using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private GameManager gameManager;

    private bool activated;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CheckpointManager.Instance.SetCheckpoint(respawnPoint.position);

        if (gameManager != null)
        {
            gameManager.OnCheckpointActivated();
        }

        if (activated) return;

        activated = true;

        animator.SetTrigger("Raise");
        animator.SetBool("Activated", true);
    }
}
