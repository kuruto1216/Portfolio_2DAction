using UnityEngine;
using TMPro;
using System.Text;

public class EndingManager : MonoBehaviour
{
    [Header("Total Text")]
    [SerializeField] private TextMeshProUGUI totalFruitText;
    [SerializeField] private TextMeshProUGUI totalDeathText;

    [Header("Stage Result")]
    [SerializeField] private TextMeshProUGUI stageResultText;

    private void Start()
    {
        ShowTotalResult();
        ShowStageResults();
    }

    private void ShowTotalResult()
    {
        if (ProgressManager.Instance == null) return;

        int collected = ProgressManager.Instance.GetTotalCollectedCount();
        int total = ProgressManager.Instance.GetTotalFruitCount();
        int totalDeaths = ProgressManager.Instance.GetTotalDeathCount();

        if (totalFruitText != null)
        {
            totalFruitText.text = $"Total Fruit : {collected} / {total}";
        }

        if (totalDeathText != null)
        {
            totalDeathText.text = $"Total Death : {totalDeaths}";
        }
    }

    private void ShowStageResults()
    {
        if (ProgressManager.Instance == null) return;
        if (stageResultText == null) return;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Stage Results");
        builder.AppendLine();

        foreach (var stage in ProgressManager.Instance.StageProgressList)
        {
            if (stage == null) continue;

            builder.AppendLine(
                $"{stage.stageId}  Fruit {stage.bestCollectedCount} / {stage.totalItemCount}" +
                $"  Death {stage.deathCount}");

            builder.AppendLine();
        }

        stageResultText.text = builder.ToString();
    }
}
