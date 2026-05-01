using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [Header("Stage Progress")]
    [SerializeField] private List<StageProgressData> stageProgressList = new List<StageProgressData>();

    [Header("Abilities")]
    [SerializeField] private PlayerAbilityData abilities = new PlayerAbilityData();

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

    private readonly HashSet<string> activatedHubCheckpointIds = new();
    private Dictionary<string, StageProgressData> stageProgressMap = new Dictionary<string, StageProgressData>();

    public bool IsArea2Unlocked() => isArea2Unlocked;
    public bool IsArea3Unlocked() => isArea3Unlocked;

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

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            Debug.Log("F5âüÇ≥ÇÍÇΩ");
            ProgressManager.Instance?.SaveGame();
        }

        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Debug.Log("F9âüÇ≥ÇÍÇΩ");
            ProgressManager.Instance?.LoadGame();
        }
    }

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

    public void SaveStageResult(string stageId, int collectedCount, int totalItemCount)
    {
        RegisterStageIfNeeded(stageId, totalItemCount);

        var data = stageProgressMap[stageId];

        data.isUnlocked = true;

        data.UpdateBest(collectedCount, totalItemCount);

        EvaluateUnlocks();
    }

    public StageProgressData GetStageProgress(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return null;

        stageProgressMap.TryGetValue(stageId, out var data);
        return data;
    }

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

    public bool IsStageUnlocked(string stageId)
    {
        var data = GetStageProgress(stageId);
        return data != null && data.isUnlocked;
    }

    private void EvaluateUnlocks()
    {
        int totalCollected = GetTotalCollectedCount();

        // âºÉãÅ[Éã
        if (totalCollected >= GetRequiredFruitForPhase(1))
        {
            abilities.canWallSlide = true;
            abilities.canWallJump = true;
        }

        if (totalCollected >= GetRequiredFruitForPhase(2))
        {
            abilities.canDoubleJump = true;
        }

        //UnlockStage("1-1");
        //UnlockStage("1-2");
        //UnlockStage("1-3");
        //UnlockStage("1-4");
        //UnlockStage("1-5");

        //if (totalCollected >= 10)
        //{
        //    UnlockStage("2-1");
        //    UnlockStage("2-2");
        //    UnlockStage("2-3");
        //    UnlockStage("2-4");
        //    UnlockStage("2-5");
        //}

        //if (totalCollected >= 20)
        //{
        //    UnlockStage("3-1");
        //    UnlockStage("3-2");
        //    UnlockStage("3-3");
        //    UnlockStage("3-4");
        //    UnlockStage("3-5");
        //}
    }

    private void UnlockStage(string stageId)
    {
        if (stageProgressMap.TryGetValue(stageId, out var data))
        {
            data.isUnlocked = true;
        }
    }

    public int GetRequiredFruitForPhase(int phase)
    {
        return phase switch
        {
            1 => phase1UnlockRequiredFruit,
            2 => phase2UnlockRequiredFruit,
            _ => 0
        };
    }

    public bool CanUnlockArea2()
    {
        return GetTotalCollectedCount() >= GetRequiredFruitForPhase(1);
    }

    public bool CanUnlockArea3()
    {
        return GetTotalCollectedCount() >= GetRequiredFruitForPhase(2);
    }

    public void UnlockArea2()
    {
        if (!CanUnlockArea2()) return;
        isArea2Unlocked = true;
    }

    public void UnlockArea3()
    {
        if (!CanUnlockArea3()) return;
        isArea3Unlocked = true;
    }

    public void SetLastHubCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId)) return;
        lastHubCheckpointId = checkpointId;
    }

    public void ActivateHubCheckpoint(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId)) return;
        activatedHubCheckpointIds.Add(checkpointId);
        lastHubCheckpointId = checkpointId;
    }

    public bool IsHubCheckpointActivated(string checkpointId)
    {
        if (string.IsNullOrEmpty(checkpointId)) return false;
        return activatedHubCheckpointIds.Contains(checkpointId);
    }
    public void ClearLastHubCheckpoint()
    {
        lastHubCheckpointId = "";
    }

    public void AddDeath(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (!stageProgressMap.TryGetValue(stageId, out var data))
        {
            return;
        }

        data.deathCount++;
    }

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

    public SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();

        saveData.totalFruit = GetTotalCollectedCount();
        saveData.totalDeaths = GetTotalDeathCount();

        saveData.lastHubCheckpointId = lastHubCheckpointId;

        saveData.dashUnlocked = abilities.canDash;
        saveData.wallJumpUnlocked = abilities.canWallJump;
        saveData.doubleJumpUnlocked = abilities.canDoubleJump;

        saveData.stages.Clear();

        foreach (var stage in stageProgressList)
        {
            if (stage == null) continue;

            StageSaveData stageSaveData = new StageSaveData();

            stageSaveData.stageId = stage.stageId;
            stageSaveData.bestFruitCount = stage.bestCollectedCount;
            stageSaveData.totalFruitCount = stage.totalItemCount;
            stageSaveData.deathCount = stage.deathCount;
            stageSaveData.isCleared = stage.bestCollectedCount > 0;

            saveData.stages.Add(stageSaveData);
        }

        saveData.activatedHubCheckpointIds.Clear();

        foreach (string id in activatedHubCheckpointIds)
        {
            saveData.activatedHubCheckpointIds.Add(id);
        }

        return saveData;
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        if (saveData == null) return;

        lastHubCheckpointId = saveData.lastHubCheckpointId;

        abilities.canDash = saveData.dashUnlocked;
        abilities.canWallJump = saveData.wallJumpUnlocked;
        abilities.canWallSlide = saveData.wallJumpUnlocked;
        abilities.canDoubleJump = saveData.doubleJumpUnlocked;

        stageProgressList.Clear();
        stageProgressMap.Clear();

        foreach (var stageSave in saveData.stages)
        {
            if (stageSave == null || string.IsNullOrEmpty(stageSave.stageId)) continue;

            StageProgressData progressData =
                new StageProgressData(stageSave.stageId, stageSave.totalFruitCount, true);

            progressData.bestCollectedCount = stageSave.bestFruitCount;
            progressData.deathCount = stageSave.deathCount;

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

    public void SaveGame()
    {
        SaveData saveData = CreateSaveData();
        SaveManager.Save(saveData);
        Debug.Log("ÉQÅ[ÉÄÇï€ë∂ÇµÇ‹ÇµÇΩ");
    }

    public void LoadGame()
    {
        SaveData saveData = SaveManager.Load();

        if (saveData == null) return;

        LoadFromSaveData(saveData);
    }

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
}
