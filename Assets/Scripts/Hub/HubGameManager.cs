using UnityEngine;

public class HubGameManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerManager player;

    [Header("Spawn")]
    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] private Checkpoint[] hubCheckpoints;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("HubGameManager: Player is not assigned.");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();

        player.RespawnTo(spawnPos, false);

        if (ProgressManager.Instance != null)
        {
            player.ApplyAbilities(ProgressManager.Instance.Abilities);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (defaultSpawnPoint == null)
        {
            Debug.LogWarning("HubGameManager: DefaultSpawnPoint is not assigned.");
            return Vector3.zero;
        }

        if (ProgressManager.Instance == null)
        {
            return defaultSpawnPoint.position;
        }

        string lastHubCheckpointId = ProgressManager.Instance.LastHubCheckpointId;

        if (string.IsNullOrEmpty(lastHubCheckpointId))
        {
            return defaultSpawnPoint.position;
        }

        foreach (var checkpoint in hubCheckpoints)
        {
            if (checkpoint == null) continue;
            if (!checkpoint.IsHubCheckpoint) continue;
            if (checkpoint.HubCheckpointId != lastHubCheckpointId) continue;
            if (checkpoint.RespawnPoint == null) continue;

            return checkpoint.RespawnPoint.position;
        }

        return defaultSpawnPoint.position;
    }
}
