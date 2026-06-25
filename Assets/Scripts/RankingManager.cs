using UnityEngine;
using unityroom.Api;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }

    private const int TotalDeathsBoardNo = 1;
    private const int Stage33TimeAttackBoardNo = 2;

    private float stage33StartTime;
    private bool isStage33TimerRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SendTotalDeaths(int totalDeaths)
    {
        UnityroomApiClient.Instance.SendScore(
            TotalDeathsBoardNo,
            totalDeaths,
            ScoreboardWriteMode.HighScoreAsc
        );
    }

    public void StartStage33Timer()
    {
        stage33StartTime = Time.time;
        isStage33TimerRunning = true;
    }

    public float GetCurrentStage33Time()
    {
        if (!isStage33TimerRunning) return 0f;

        return Time.time - stage33StartTime;
    }

    public void SendStage33TimeAttack()
    {
        if (!isStage33TimerRunning) return;

        float clearTime = Time.time - stage33StartTime;
        isStage33TimerRunning = false;

        UnityroomApiClient.Instance.SendScore(
            Stage33TimeAttackBoardNo,
            clearTime,
            ScoreboardWriteMode.HighScoreAsc
        );
    }
}
