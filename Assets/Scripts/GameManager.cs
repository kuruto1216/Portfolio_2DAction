using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject gameClearText;
    [SerializeField] TextMeshProUGUI scoreText;

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
        Invoke("RestartScene", 1.5f);
    }

    public void GameClear()
    {
        gameClearText.SetActive(true);
        Invoke("RestartScene", 1.5f);
    }

    void RestartScene()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }
}
