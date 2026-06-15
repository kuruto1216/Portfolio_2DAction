using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject gameClearText;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Respawn")]
    [SerializeField] PlayerManager player;
    [SerializeField] Transform defaultSpawnPoint;

    [Header("Scene")]
    [SerializeField] private string hubSceneName = "Hub";

    [Header("Stage Info")]
    [SerializeField] private string stageId = "1-1";
    [SerializeField] private int totalItemsInStage = 5;
    [SerializeField] private bool registerAsUnlockedByDefault = true;

    [Header("Clear")]
    [SerializeField] private string nextSceneName = "Hub";

    [Header("SE")]
    [SerializeField] private AudioClip clearSE;
    [SerializeField, Range(0f, 1f)] private float clearSEVolume = 1f;

    private int committedCollectedCount = 0;
    private int tempCollectedCount = 0;
    private bool isGameOver = false;

    private readonly List<ItemManager> tempCollectedItems = new();
    private readonly List<ItemManager> committedCollectedItems = new();

    private void Start()
    {
        UpdateScoreText();
        player.RespawnTo(defaultSpawnPoint.position, false);

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RegisterStageIfNeeded(stageId, totalItemsInStage, registerAsUnlockedByDefault);

            player.ApplyAbilities(ProgressManager.Instance.Abilities);
        }
    }

    public void CollectItem(ItemManager item)
    {
        if (item == null) return;
        if (tempCollectedItems.Contains(item)) return;
        if (committedCollectedItems.Contains(item)) return;

        item.Collect();
        tempCollectedItems.Add(item);
        tempCollectedCount++;

        ClampCounts();
        UpdateScoreText();
    }

    public void CommitTempItems()
    {
        if (tempCollectedItems.Count == 0) return;

        for (int i = 0; i < tempCollectedItems.Count; i++)
        {
            ItemManager item = tempCollectedItems[i];
            if (item == null) continue;

            item.Commit();

            if (!committedCollectedItems.Contains(item))
            {
                committedCollectedItems.Add(item);
            }
        }

        committedCollectedCount += tempCollectedCount;
        tempCollectedCount = 0;
        tempCollectedItems.Clear();

        ClampCounts();
        UpdateScoreText();
    }

    public void ResetTempItems()
    {
        for (int i = 0; i < tempCollectedItems.Count; i++)
        {
            ItemManager item = tempCollectedItems[i];
            if (item == null) continue;

            item.ResetUncommitted();
        }

        tempCollectedItems.Clear();
        tempCollectedCount = 0;
        UpdateScoreText();
    }

    public int GetCurrentVisibleCollectedCount()
    {
        return committedCollectedCount + tempCollectedCount;
    }

    public void OnCheckpointActivated()
    {
        CommitTempItems();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.AddDeath(stageId);
        }

        gameOverText.SetActive(true);
        Invoke(nameof(Respawn), 1.5f);
    }

    public void GameClear()
    {
        CommitTempItems();      // Goal“ž’B‚Å–¢Šm’è•ª‚àŠm’è

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SaveStageResult(stageId, committedCollectedCount, totalItemsInStage);

            ProgressManager.Instance.SaveGame();
        }

        gameClearText.SetActive(true);
        AudioManager.Instance.PlaySE(clearSE, clearSEVolume);
        Invoke(nameof(GoToNextScene), 1.5f);
    }

    public void RestartFromCheckpointByMenu()
    {
        if (isGameOver) return;

        gameOverText.SetActive(false);

        ResetTempItems();

        Vector3 spawnPos = defaultSpawnPoint.position;

        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            spawnPos = CheckpointManager.Instance.RespawnPosition;
        }

        player.RespawnTo(spawnPos);

        isGameOver = false;
    }

    private void Respawn()
    {
        gameOverText.SetActive(false);

        ResetTempItems();

        Vector3 spawnPos = defaultSpawnPoint.position;
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            spawnPos = CheckpointManager.Instance.RespawnPosition;
        }

        player.RespawnTo(spawnPos);

        isGameOver = false;
    }

    private void GoToNextScene()
    {
        TransitionManager.Instance.LoadScene(nextSceneName);
    }

    private void ClampCounts()
    {
        if (committedCollectedCount > totalItemsInStage)
        {
            committedCollectedCount = totalItemsInStage;
        }

        if (tempCollectedCount > totalItemsInStage)
        {
            tempCollectedCount = totalItemsInStage;
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            if (tempCollectedCount > 0)
            {
                scoreText.text = $"{committedCollectedCount}<size=80%><color=#00AFFF>(+{tempCollectedCount})</color></size> / {totalItemsInStage}";
            }
            else
            {
                scoreText.text = $"{committedCollectedCount} / {totalItemsInStage}";
            }
        }
    }
}
