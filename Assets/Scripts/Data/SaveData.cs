using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // ゲーム全体のFruit総取得・Death総数
    public int totalFruit;
    public int totalDeaths;

    // 最後に触れたHubのCheckpoint保存
    public string lastHubCheckpointId;

    // Player能力の解放状態
    public bool dashUnlocked;
    public bool wallJumpUnlocked;
    public bool doubleJumpUnlocked;

    // Hubのゲート解放状態
    public bool isArea2Unlocked;
    public bool isArea3Unlocked;

    // 一度触れたCheckpointを保存
    public List<string> activatedHubCheckpointIds = new List<string>();

    // 各ステージごとの成績を保存
    public List<StageSaveData> stages = new List<StageSaveData>();
}

[System.Serializable]
public class StageSaveData
{
    public string stageId;
    public int bestFruitCount;
    public int totalFruitCount;
    public int deathCount;

    public bool isCleared;
}
