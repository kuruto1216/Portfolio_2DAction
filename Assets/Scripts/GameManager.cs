using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject gameClearText;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Respawn")]
    [SerializeField] PlayerManager player;
    [SerializeField] Transform defaultSpawnPoint;

    const int MAX_SCORE = 99;
    int score = 0;

    private void Start()
    {
        scoreText.text = score.ToString();
    }
    public void AddScore()
    {
        score += 1;

        if (score > MAX_SCORE)
        {
            score = MAX_SCORE;
        }

        scoreText.text = score.ToString();
    }

    public void GameOver()
    {
        gameOverText.SetActive(true);
        Invoke(nameof(Respawn), 1.5f);
    }

    public void GameClear()
    {
        gameClearText.SetActive(true);
        Invoke(nameof(RestartScene), 1.5f);
    }

    void Respawn()
    {
        gameOverText.SetActive(false);

        Vector3 spawnPos = defaultSpawnPoint.position;
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            spawnPos = CheckpointManager.Instance.RespawnPosition;
        }

        player.RespawnTo(spawnPos);
    }

    void RestartScene()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }
}
