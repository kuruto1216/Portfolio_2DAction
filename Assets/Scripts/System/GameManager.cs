using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ===== 変数 =====

    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject gameClearText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI clearTimeText;

    [Header("Respawn")]
    [SerializeField] PlayerManager player;
    [SerializeField] Transform defaultSpawnPoint;

    [Header("Scene")]
    [SerializeField] private string hubSceneName = "Hub";

    [Header("Stage Info")]
    [SerializeField] private string stageId = "1-1";    // ProgressManagerで使用するステージID
    [SerializeField] private int totalItemsInStage = 5; // ステージ内のFruit総数
    [SerializeField] private bool registerAsUnlockedByDefault = true;   // 初回登録時に解放済みとして扱う

    [Header("Clear")]
    [SerializeField] private string nextSceneName = "Hub";  // クリア後の遷移先

    [Header("SE")]
    [SerializeField] private AudioClip clearSE;
    [SerializeField, Range(0f, 1f)] private float clearSEVolume = 1f;

    private int committedCollectedCount = 0;    // 確定済みのFruit数
    private int tempCollectedCount = 0;         // 仮取得のFruit数

    private bool isGameOver = false;    // ゲームオーバー処理中かの判定

    private readonly List<ItemManager> tempCollectedItems = new();          // 仮取得中のFruit一覧
    private readonly List<ItemManager> committedCollectedItems = new();     // 確定済みのFruit一覧


    // ===== Unityイベント =====

    private void Start()
    {
        UpdateScoreText();
        player.RespawnTo(defaultSpawnPoint.position, false);

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RegisterStageIfNeeded(stageId, totalItemsInStage, registerAsUnlockedByDefault);

            player.ApplyAbilities(ProgressManager.Instance.Abilities);
        }

        if (stageId == "3-3" && RankingManager.Instance != null)
        {
            RankingManager.Instance.StartStage33Timer();
        }

        if (clearTimeText != null)
        {
            clearTimeText.gameObject.SetActive(false);
        }
    }

    // ===== Fruit取得管理 =====

    // Fruitを取得する
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

    // 仮取得Fruitを確定する(チェックポイント・Goal到達時)
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

        // 確定数への加算と仮取得情報の削除
        committedCollectedCount += tempCollectedCount;
        tempCollectedCount = 0;
        tempCollectedItems.Clear();

        ClampCounts();
        UpdateScoreText();
    }

    // 仮取得Fruitをリセットする(死亡時・チェックポイント再開時)
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

    // 現在画面に表示するFruit取得数を返す
    public int GetCurrentVisibleCollectedCount()
    {
        return committedCollectedCount + tempCollectedCount;
    }

    // ===== チェックポイント =====

    // チェックポイント到達時の処理
    public void OnCheckpointActivated()
    {
        CommitTempItems();
    }

    // ===== ゲーム進行 =====

    // ゲームオーバー処理を行う
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

    // ゲームクリア処理を行う
    public void GameClear()
    {
        CommitTempItems();      // Goal到達で未確定分も確定

        bool wasAlreadyCleared = false;

        // Stage3-3ならクリアタイムを表示(unityroom用実装)
        if (stageId == "3-3" && RankingManager.Instance != null)
        {
            float clearTime = RankingManager.Instance.GetCurrentStage33Time();
            ShowClearTime(clearTime);
        }

        if (ProgressManager.Instance != null)
        {
            StageProgressData progress = ProgressManager.Instance.GetStageProgress(stageId);
            wasAlreadyCleared = progress != null && progress.isCleared;

            ProgressManager.Instance.SaveStageResult(stageId, committedCollectedCount, totalItemsInStage);
            ProgressManager.Instance.SaveGame();

            // Stage3-3ならランキング送信(unityroom用実装)
            if (stageId == "3-3" && RankingManager.Instance != null)
            {
                RankingManager.Instance.SendTotalDeaths(
                    ProgressManager.Instance.GetTotalDeathCount()
                );

                RankingManager.Instance.SendStage33TimeAttack();
            }
        }

        // Stage3-3クリア済みならEndingなしでHubへ
        if (stageId == "3-3" && wasAlreadyCleared)
        {
            nextSceneName = hubSceneName;
        }

        gameClearText.SetActive(true);
        AudioManager.Instance.PlaySE(clearSE, clearSEVolume);
        Invoke(nameof(GoToNextScene), 1.5f);
    }

    // ===== リスポーン =====

    // チェックポイントから再開する
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

    // プレイヤーを復活させる
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

    // ===== UI更新 =====

    // クリアタイムを表示する(stage3-3のみ利用)
    private void ShowClearTime(float time)
    {
        if (clearTimeText == null) return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        clearTimeText.text = $"Time：{minutes:00}:{seconds:00}";
        clearTimeText.gameObject.SetActive(true);
    }

    // Fruit数表示を更新する
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            if (tempCollectedCount > 0)
            {
                scoreText.text = $"{committedCollectedCount}<size=80%><color=#00AFFF>" +
                    $"(+{tempCollectedCount})</color></size> / {totalItemsInStage}";
            }
            else
            {
                scoreText.text = $"{committedCollectedCount} / {totalItemsInStage}";
            }
        }
    }

    // ===== シーン遷移 =====

    // 次のシーンへ遷移する
    private void GoToNextScene()
    {
        TransitionManager.Instance.LoadScene(nextSceneName);
    }

    // ===== 補助処理 =====

    // Fruit数が上限を超えないよう補正する
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
}
