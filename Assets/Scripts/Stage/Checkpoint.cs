using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private GameManager gameManager;

    [Header("Hub Checkpoint")]
    [SerializeField] private bool isHubCheckpoint = false;
    [SerializeField] private string hubCheckpointId = "";

    private bool activated;

    public Transform RespawnPoint => respawnPoint;
    public bool IsHubCheckpoint => isHubCheckpoint;
    public string HubCheckpointId => hubCheckpointId;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (!isHubCheckpoint && gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    private void Start()
    {
        RestoreHubCheckpointVisual();
    }

    private void RestoreHubCheckpointVisual()
    {
        if (!isHubCheckpoint) return;
        if (animator == null) return;
        if (ProgressManager.Instance == null) return;

        if (ProgressManager.Instance.IsHubCheckpointActivated(hubCheckpointId))
        {
            activated = true;
            animator.Play("FlagIdleAnimation", 0, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isHubCheckpoint)
        {
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.ActivateHubCheckpoint(hubCheckpointId);
            }
        }
        else
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetCheckpoint(respawnPoint.position);
            }

            if (gameManager != null)
            {
                gameManager.OnCheckpointActivated();
            }
        }

        if (activated) return;

        activated = true;

        if (animator != null)
        {
            animator.SetTrigger("Raise");
        }
    }
}
