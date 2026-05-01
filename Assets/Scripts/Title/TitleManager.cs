using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button continueButton;

    [Header("Scene")]
    [SerializeField] private string hubSceneName = "Hub";

    private void Start()
    {
        // Continueボタンの有無切り替え
        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.HasSaveData();
        }
    }

    public void OnNewGame()
    {
        SaveManager.DeleteSaveData();   // 既存のセーブデータ削除

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene(hubSceneName);
    }

    public void OnContinue()
    {
        if (!SaveManager.HasSaveData()) return;

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.LoadGame();
        }

        SceneManager.LoadScene(hubSceneName);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
