using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class TitleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;

    [Header("Scene")]
    [SerializeField] private string hubSceneName = "Hub";

    [Header("First Selected")]
    [SerializeField] private GameObject firstSelectedButton;

    [Header("Options")]
    [SerializeField] private GameObject titleMenuRoot;
    [SerializeField] private OptionsManager optionsManager;

    private bool hasSaveData;

    [Header("Review Mode")]
    [SerializeField] private GameObject reviewModeRoot;
    [SerializeField] private Button area2StartButton;

    private readonly Key[] reviewCommand =
    {
        Key.R,
        Key.E,
        Key.V,
        Key.I,
        Key.E,
        Key.W
    };

    private int reviewCommandIndex;

    private void Start()
    {
        hasSaveData = SaveManager.HasSaveData();

        // Continueボタンの有無切り替え
        if (continueButton != null)
        {
            continueButton.interactable = hasSaveData;
        }

        if (reviewModeRoot != null)
        {
            reviewModeRoot.SetActive(false);
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

        CheckReviewCommand();
    }

    private void SelectFirstButton()
    {
        if (EventSystem.current == null) return;

        GameObject selectedButton = null;

        if (hasSaveData && continueButton != null && continueButton.interactable)
        {
            selectedButton = continueButton.gameObject;
        }
        else if (newGameButton != null)
        {
            selectedButton = newGameButton.gameObject;
        }
        else
        {
            selectedButton = firstSelectedButton;
        }

        if (selectedButton == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectedButton);
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

    public void OnOptions()
    {
        titleMenuRoot.SetActive(false);
        optionsManager.OpenOptions();
    }

    public void OnCloseOptions()
    {
        optionsManager.CloseOptions();
        titleMenuRoot.SetActive(true);
        SelectFirstButton();
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    private void CheckReviewCommand()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[reviewCommand[reviewCommandIndex]].wasPressedThisFrame)
        {
            reviewCommandIndex++;

            if (reviewCommandIndex >= reviewCommand.Length)
            {
                OpenReviewMode();
                reviewCommandIndex = 0;
            }
        }
        else if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            reviewCommandIndex = 0;
        }
    }

    private void OpenReviewMode()
    {
        if (reviewModeRoot == null) return;

        reviewModeRoot.SetActive(true);

        if (area2StartButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(area2StartButton.gameObject);
        }
    }

    public void OnReviewArea2Start()
    {
        ProgressManager.Instance.CreateReviewArea2Progress();
        ProgressManager.Instance.SaveGame();

        TransitionManager.Instance.LoadScene(hubSceneName);
    }

    public void OnReviewArea3Start()
    {
        ProgressManager.Instance.CreateReviewArea3Progress();
        ProgressManager.Instance.SaveGame();

        TransitionManager.Instance.LoadScene(hubSceneName);
    }
}
