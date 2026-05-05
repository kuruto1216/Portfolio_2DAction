using UnityEngine;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    [SerializeField] private float slowScale = 0.1f;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        // 1キーでスロー
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            Time.timeScale = slowScale;
        }

        // 2キーで通常速度
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
        }
    }
}
