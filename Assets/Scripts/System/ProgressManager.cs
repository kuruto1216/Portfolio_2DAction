using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    // ===== 変数 =====

    // ステージごとの進行状況一覧
    [Header("Stage Progress")]
    [SerializeField] private List<StageProgressData> stageProgressList = new List<StageProgressData>();

    // プレイヤーの能力解放状態
    [Header("Abilities")]
    [SerializeField] private PlayerAbilityData abilities = new PlayerAbilityData();

    // 最後に有効化したHubチェックポイントIDを保持する
    [Header("Hub Respawn")]
    [SerializeField] private string lastHubCheckpointId = "";

    [SerializeField] private bool isArea2Unlocked;
    [SerializeField] private bool isArea3Unlocked;

    [Header("Unlock Requirements")]
    [SerializeField] private int phase1UnlockRequiredFruit = 10;
    [SerializeField] private int phase2UnlockRequiredFruit = 20;

    public IReadOnlyList<StageProgressData> StageProgressList => stageProgressList;
    public PlayerAbilityData Abilities => abilities;
    public string LastHubCheckpointId => lastHubCheckpointId;

    // 一度有効化したHubチェックポイントIDを保持する
    private readonly HashSet<string> activatedHubCheckpointIds = new();

    // stageIDをキーにしてStageProgressDataを高速検索するDictionary
    private Dictionary<string, StageProgressData> stageProgressMap = new Dictionary<string, StageProgressData>();

    public bool IsArea2Unlocked() => isArea2Unlocked;
    public bool IsArea3Unlocked() => isArea3Unlocked;

    // ===== Unityイベント =====

    // Singletonの初期化と永続化
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RebuildDictionary();
    }

    // ===== StageProgress管理 =====

    // ListからDictionaryを再構築する
    private void RebuildDictionary()
    {
        stageProgressMap.Clear();

        foreach (var data in stageProgressList)
        {
            if (data == null || string.IsNullOrEmpty(data.stageId)) continue;

            if (!stageProgressMap.ContainsKey(data.stageId))
            {
                stageProgressMap.Add(data.stageId, data);
            }
        }
    }

    // ステージ情報が未登録なら新規登録する
    public void RegisterStageIfNeeded(string stageId, int totalItemCount, bool defaultUnlocked = false)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (stageProgressMap.TryGetValue(stageId, out var data))
        {
            data.totalItemCount = totalItemCount;
            return;
        }

        var newData = new StageProgressData(stageId, totalItemCount, defaultUnlocked);
        stageProgressList.Add(newData);
        stageProgressMap.Add(stageId, newData);
    }

    // ステージの進行状況を取得する
    public StageProgressData GetStageProgress(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return null;

        stageProgressMap.TryGetValue(stageId, out var data);
        return data;
    }

    // ステージクリア結果を保存・更新する
    public void SaveStageResult(string stageId, int collectedCount, int totalItemCount)
    {
        RegisterStageIfNeeded(stageId, totalItemCount);

        var data = stageProgressMap[stageId];

        data.isUnlocked = true;
        data.isCleared = true;

        data.UpdateBest(collectedCount, totalItemCount);

        EvaluateUnlocks();
    }

    // 指定ステージの死亡回数を加算する
    public void AddDeath(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (!stageProgressMap.TryGetValue(stageId, out var data))
        {
            return;
        }

        data.deathCount++;
    }

    // ステージ解放済みかの判定(未使用)
    public bool IsStageUnlocked(string stageId)
    {
        var data = GetStageProgress(stageId);
        return data != null && data.isUnlocked;
    }

    // ===== 集計系 =====

    // 全ステージのFruit取得数を合計する
    public int GetTotalCollectedCount()
    {
        int total = 0;

        foreach (var data in stageProgressList)
        {
            if (data == null) continue;

            total += data.bestCollectedCount;
        }

        return total;
    }

    // 全ステージのFruit総数を取得する
    public int GetTotalFruitCount()
    {
        int total = 0;

        foreach (var stage in stageProgressList)
        {
            if (stage == null) continue;

            total += stage.totalItemCount;
        }

        return total;
    }

    // 全ステージの死亡回数を合計する
    public int GetTotalDeathCount()
    {
        int total = 0;

        foreach (var data in stageProgressList)
        {
            if (data == null) continue;

            total += data.deathCount;
        }

        return total;
    }

    // ===== 能力・解放関連 =====

    // Fruit取得数に応じて能力解放状態を更新する
    private void EvaluateUnlocks()
    {
        int totalCollected = GetTotalCollectedCount();

        if (totalCollected >= GetRequiredFruitForPhase(1))
        {
            abilities.canWallSlide = true;
            abilities.canWallJump = true;
        }

        if (totalCollected >= GetRequiredFruitForPhase(2))
        {
            abilities.canDoubleJump = true;
        }
    }

    // フェーズごとの必要Fruit数を取得する
    public int GetRequiredFruitForPhase(int phase)
    {
        return phase switch
        {
            1 => phase1UnlockRequiredFruit,
            2 => phase2UnlockRequiredFruit,
            _ => 0
        };
    }

    // Area2を開放可能か判定する
    public bool CanUnlockArea2()
    {
        return GetTotalCollectedCount() >= GetRequiredFruitForPhase(1);
    }

    // Area3を開放可能か判定する
    public bool CanUnlockArea3()
    {
        return GetTotalCollectedCount() >= GetRequiredFruitForPhase(2);
    }

    // Area2を開放する
    public void UnlockArea2()
    {
        if (!CanUnlockArea2()) return;
        isArea2Unlocked = true;
    }

    // Area3を開放する
    public void UnlockArea3()
    {
        if (!CanUnlockArea3()) return;
        isArea3Unlocked = true;
    }

    // 指定ステージを開放状態にする(未使用)
    private void UnlockStage(string stageId)
    {
        if (stageProgressMap.TryGetValue(stageId, out var data))
        {
            data.isUnlocked = true;
        }
    }

    // ===== Hubチェックポイント =====

    // 最後に通過したHubチェックポイントを設定する
    public void SetLastHubCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId)) return;
        lastHubCheckpointId = checkpointId;
    }

    // Hubチェックポイントを有効化して記録する
    public void ActivateHubCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId)) return;
        activatedHubCheckpointIds.Add(checkpointId);
        lastHubCheckpointId = checkpointId;
    }

    // Hubチェックポイントが有効か判定する
    public bool IsHubCheckpointActivated(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId)) return false;
        return activatedHubCheckpointIds.Contains(checkpointId);
    }

    // 最後のHubチェックポイント情報をクリアする
    public void ClearLastHubCheckpoint()
    {
        lastHubCheckpointId = "";
    }

    // ===== セーブ・ロード =====

    // 現在の進行状況からセーブデータを作成する
    public SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();

        saveData.totalFruit = GetTotalCollectedCount();
        saveData.totalDeaths = GetTotalDeathCount();

        saveData.lastHubCheckpointId = lastHubCheckpointId;

        saveData.dashUnlocked = abilities.canDash;
        saveData.wallJumpUnlocked = abilities.canWallJump;
        saveData.doubleJumpUnlocked = abilities.canDoubleJump;

        saveData.isArea2Unlocked = isArea2Unlocked;
        saveData.isArea3Unlocked = isArea3Unlocked;

        saveData.stages.Clear();

        foreach (var stage in stageProgressList)
        {
            if (stage == null) continue;

            StageSaveData stageSaveData = new StageSaveData();

            stageSaveData.stageId = stage.stageId;
            stageSaveData.bestFruitCount = stage.bestCollectedCount;
            stageSaveData.totalFruitCount = stage.totalItemCount;
            stageSaveData.deathCount = stage.deathCount;
            stageSaveData.isCleared = stage.isCleared;

            saveData.stages.Add(stageSaveData);
        }

        saveData.activatedHubCheckpointIds.Clear();

        foreach (string id in activatedHubCheckpointIds)
        {
            saveData.activatedHubCheckpointIds.Add(id);
        }

        return saveData;
    }

    // セーブデータを現在の進行状況に反映する
    public void LoadFromSaveData(SaveData saveData)
    {
        if (saveData == null) return;

        lastHubCheckpointId = saveData.lastHubCheckpointId;

        abilities.canDash = saveData.dashUnlocked;
        abilities.canWallJump = saveData.wallJumpUnlocked;
        abilities.canWallSlide = saveData.wallJumpUnlocked;
        abilities.canDoubleJump = saveData.doubleJumpUnlocked;

        isArea2Unlocked = saveData.isArea2Unlocked;
        isArea3Unlocked = saveData.isArea3Unlocked;

        stageProgressList.Clear();
        stageProgressMap.Clear();

        foreach (var stageSave in saveData.stages)
        {
            if (stageSave == null || string.IsNullOrEmpty(stageSave.stageId)) continue;

            StageProgressData progressData =
                new StageProgressData(stageSave.stageId, stageSave.totalFruitCount, true);

            progressData.bestCollectedCount = stageSave.bestFruitCount;
            progressData.deathCount = stageSave.deathCount;
            progressData.isCleared = stageSave.isCleared;

            stageProgressList.Add(progressData);
            stageProgressMap.Add(progressData.stageId, progressData);
        }

        activatedHubCheckpointIds.Clear();

        foreach (string id in saveData.activatedHubCheckpointIds)
        {
            if (string.IsNullOrEmpty(id)) continue;
            activatedHubCheckpointIds.Add(id);
        }

        EvaluateUnlocks();
    }

    // 現在の進行状況を保存する
    public void SaveGame()
    {
        SaveData saveData = CreateSaveData();
        SaveManager.Save(saveData);
        Debug.Log("ゲームを保存しました");
    }

    // セーブデータを読み込む
    public void LoadGame()
    {
        SaveData saveData = SaveManager.Load();

        if (saveData == null) return;

        LoadFromSaveData(saveData);
    }

    // ===== リセット =====

    // 進行状況を初期状態にリセットする
    public void ResetProgress()
    {
        stageProgressList.Clear();
        stageProgressMap.Clear();

        abilities = new PlayerAbilityData();

        lastHubCheckpointId = "";
        activatedHubCheckpointIds.Clear();

        isArea2Unlocked = false;
        isArea3Unlocked = false;
    }

    // ===== Review Mode用 =====

    public void CreateReviewArea2Progress()
    {
        ResetProgress();

        abilities.canDash = true;
        abilities.canWallSlide = true;
        abilities.canWallJump = true;

        isArea2Unlocked = true;

        ActivateHubCheckpoint("Area2");
    }

    public void CreateReviewArea3Progress()
    {
        ResetProgress();

        abilities.canDash = true;
        abilities.canWallSlide = true;
        abilities.canWallJump = true;
        abilities.canDoubleJump = true;

        isArea2Unlocked = true;
        isArea3Unlocked = true;

        ActivateHubCheckpoint("Area3");
    }
}
