using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Next Scene Name")]
    [SerializeField] private string nextSceneName;

    [Header("Input")]
    [SerializeField] private Key key = Key.Space;

    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[key].wasPressedThisFrame)
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("nextSceneName が空です。Inspectorで次のシーン名を設定してください。");
                return;
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
