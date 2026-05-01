using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int totalFruit;
    public int totalDeaths;

    public string lastHubCheckpointId;

    public bool dashUnlocked;
    public bool wallJumpUnlocked;
    public bool doubleJumpUnlocked;

    public List<string> activatedHubCheckpointIds = new List<string>();

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
