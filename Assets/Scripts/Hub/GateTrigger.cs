using UnityEngine;
using System.Collections;
using TMPro;

public class GateTrigger : MonoBehaviour
{
    [SerializeField] private GateController gateController;

    [Header("UI")]
    [SerializeField] private GameObject infoRoot;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform infoPanelRect;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.1f;
    [SerializeField] private float floatOffsetY = 1f;

    private PlayerManager currentPlayer;
    private Coroutine showHideCo;
    private Vector2 shownPos;
    private Vector2 hiddenPos;

    private void Awake()
    {
        if (gateController == null)
        {
            gateController = GetComponentInParent<GateController>();
        }

        if (canvasGroup == null && infoRoot != null)
        {
            canvasGroup = infoRoot.GetComponent<CanvasGroup>();
        }

        if (infoPanelRect == null && infoRoot != null)
        {
            infoPanelRect = infoRoot.GetComponentInChildren<RectTransform>();
        }

        if (infoPanelRect != null)
        {
            shownPos = infoPanelRect.anchoredPosition;
            hiddenPos = shownPos + new Vector2(0f, -floatOffsetY);
        }

        SetHiddenImmediate();
    }

    private void OnDisable()
    {
        SetHiddenImmediate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm == null) return;

        currentPlayer = pm;
        pm.SetCurrentGate(this);
        RefreshInfo();
        PlayShow();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm == null) return;

        if (currentPlayer == pm)
        {
            currentPlayer = null;
        }

        pm.ClearCurrentGate(this);
        SetHiddenImmediate();
    }

    public void TryUnlock()
    {
        if (gateController == null) return;

        gateController.TryUnlock();

        if (currentPlayer != null && ProgressManager.Instance != null)
        {
            currentPlayer.RefreshAbilities();
        }

        RefreshInfo();
    }

    public bool CanUnlock()
    {
        return gateController != null && gateController.CanUnlock();
    }

    private void RefreshInfo()
    {
        int current = ProgressManager.Instance != null
            ? ProgressManager.Instance.GetTotalCollectedCount()
            : 0;

        int required = gateController != null
            ? gateController.GetRequiredFruitCount()
            : 0;

        if (progressText != null)
        {
            string currentText = current >= required
                ? $"{current}"
                : $"<color=#00AFFF>{current}</color>";

            progressText.text = $"{currentText} / {required}";
        }

        if (actionText != null)
        {
            if (gateController != null && gateController.IsUnlocked())
            {
                actionText.text = "âï˙çœÇ›";
            }
            else if (CanUnlock())
            {
                actionText.text = "EÇ≈âï˙";
            }
            else
            {
                actionText.text = "";
            }
        }
    }

    private void PlayShow()
    {
        if (showHideCo != null)
        {
            StopCoroutine(showHideCo);
            showHideCo = null;
        }

        if (infoRoot != null)
        {
            infoRoot.SetActive(true);
        }

        if (!isActiveAndEnabled) return;

        showHideCo = StartCoroutine(CoFadeMove(0f, 1f, hiddenPos, shownPos));
    }

    private IEnumerator CoFadeMove(float fromAlpha, float toAlpha, Vector2 fromPos, Vector2 toPos)
    {
        if (canvasGroup == null || infoPanelRect == null)
        {
            yield break;
        }

        float time = 0f;
        canvasGroup.alpha = fromAlpha;
        infoPanelRect.anchoredPosition = fromPos;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            float eased = t * t * (3f - 2f * t);

            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
            infoPanelRect.anchoredPosition = Vector2.Lerp(fromPos, toPos, eased);

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
        infoPanelRect.anchoredPosition = toPos;
    }

    private void SetHiddenImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (infoPanelRect != null)
        {
            infoPanelRect.anchoredPosition = hiddenPos;
        }

        if (infoRoot != null)
        {
            infoRoot.SetActive(false);
        }
    }
}
