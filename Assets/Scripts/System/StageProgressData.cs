using System;

[Serializable]
public class StageProgressData
{
    public string stageId;
    public int bestCollectedCount;
    public int totalItemCount;
    public int deathCount;

    public bool isUnlocked;     // –¢Žg—p

    public StageProgressData(string stageId, int totalItemCount, bool isUnlocked = false)
    {
        this.stageId = stageId;
        this.totalItemCount = totalItemCount;
        this.bestCollectedCount = 0;
        this.isUnlocked = isUnlocked;
    }

    public void UpdateBest(int collectedCount, int totalCount)
    {
        totalItemCount = totalCount;

        if (collectedCount > bestCollectedCount)
        {
            bestCollectedCount = collectedCount;
        }

        if (bestCollectedCount > totalItemCount)
        {
            bestCollectedCount = totalItemCount;
        }
    }
}
