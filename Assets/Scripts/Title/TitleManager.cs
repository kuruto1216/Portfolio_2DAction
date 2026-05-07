using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class TitleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button continueButton;

    [Header("Scene")]
    [SerializeField] private string hubSceneName = "Hub";

    [Header("First Selected")]
    [SerializeField] private GameObject firstSelectedButton;

    private void Start()
    {
        // Continueボタンの有無切り替え
        if (continueButton != null)
        {
            continueButton.interactable = SaveManager.HasSaveData();
        }

        SelectFirstButton();
    }

    private void Update()
    {
        if (EventSystem.current == null) return;
        if (EventSystem.current.currentSelectedGameObject != null) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                Keyboard.current.downArrowKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame)
            {
                SelectFirstButton();
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.up.wasPressedThisFrame ||
                Gamepad.current.dpad.down.wasPressedThisFrame ||
                Gamepad.current.leftStick.up.wasPressedThisFrame ||
                Gamepad.current.leftStick.down.wasPressedThisFrame)
            {
                SelectFirstButton();
            }
        }
    }

    private void SelectFirstButton()
    {
        if (EventSystem.current == null) return;
        if (firstSelectedButton == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void OnNewGame()
    {
        SaveManager.DeleteSaveData();   // 既存のセーブデータ削除

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.ResetProgress();
        }

        TransitionManager.Instance.LoadScene(hubSceneName);
    }

    public void OnContinue()
    {
        if (!SaveManager.HasSaveData()) return;

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.LoadGame();
        }

        TransitionManager.Instance.LoadScene(hubSceneName);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
