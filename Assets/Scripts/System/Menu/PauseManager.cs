using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button firstSelectedButton;
    [SerializeField] private GameObject retryStageButton;
    [SerializeField] private GameObject returnHubButton;

    [Header("Scene Names")]
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private string titleSceneName = "Title";

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string playerActionMapName = "Player";

    [Header("Options")]
    [SerializeField] private OptionsManager optionsManager;

    private bool isPaused;
    private bool isOptionsOpen;

    private void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        SetupButtonsByScene();
    }

    private void OnEnable()
    {
        pauseAction.action.performed += OnPausePerformed;
        pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePerformed;
        pauseAction.action.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void SetupButtonsByScene()
    {
        bool isHub = SceneManager.GetActiveScene().name == hubSceneName;

        retryStageButton.SetActive(!isHub);
        returnHubButton.SetActive(!isHub);
    }

    public void TogglePause()
    {
        if (isOptionsOpen)
        {
            CloseOptions();
            return;
        }

        if (isPaused)
        {
            Resume();
        }
        else
        {
            OpenPause();
        }
    }

    private void OpenPause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        playerInput.currentActionMap.Disable();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    public void Resume()
    {
        isOptionsOpen = false;

        isPaused = false;
        pausePanel.SetActive(false);
        optionsManager.CloseOptions();

        Time.timeScale = 1f;

        playerInput.SwitchCurrentActionMap(playerActionMapName);
    }

    public void RetryStage()
    {
        Time.timeScale = 1f;

        string currentSceneName = SceneManager.GetActiveScene().name;
        TransitionManager.Instance.LoadScene(currentSceneName);
    }

    public void OpenOptions()
    {
        isOptionsOpen = true;

        pausePanel.SetActive(false);
        optionsManager.OpenOptions();
    }

    public void CloseOptions()
    {
        isOptionsOpen = false;

        optionsManager.CloseOptions();
        pausePanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject (firstSelectedButton.gameObject);
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;

        TransitionManager.Instance.LoadScene(hubSceneName);
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        TransitionManager.Instance.LoadScene(titleSceneName);
    }
}
