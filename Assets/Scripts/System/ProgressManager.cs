using UnityEngine;
using System.Collections.Generic;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [Header("Stage Progress")]
    [SerializeField] private List<StageProgressData> stageProgressList = new List<StageProgressData>();

    [Header("Abilities")]
    [SerializeField] private PlayerAbilityData abilities = new PlayerAbilityData();

    [SerializeField] private bool isArea2Unlocked;
    [SerializeField] private bool isArea3Unlocked;

    [Header("Unlock Requirements")]
    [SerializeField] private int phase1UnlockRequiredFruit = 10;
    [SerializeField] private int phase2UnlockRequiredFruit = 20;


    public IReadOnlyList<StageProgressData> StageProgressList => stageProgressList;
    public PlayerAbilityData Abilities => abilities;

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

        // ‰¼ƒ‹[ƒ‹
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
}
