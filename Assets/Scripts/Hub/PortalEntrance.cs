using UnityEngine;
using TMPro;
using System.Collections;

public class PortalEntrance : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string sceneName;

    [Header("Stage Info")]
    [SerializeField] private string stageId;

    [Header("UI")]
    [SerializeField] private GameObject infoRoot;
    [SerializeField] private TextMeshProUGUI stageIdText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform infoPanelRect;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float floatOffsetY = 10f;

    public string SceneName => sceneName;
    public string StageId => stageId;

    private Coroutine showHideCo;
    private Vector2 shownPos;
    private Vector2 hiddenPos;

    private void Awake()
    {
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm != null)
        {
            pm.SetCurrentPortal(this);
            RefreshInfo();
            PlayShow();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm != null)
        {
            pm.ClearCurrentPortal(this);
            PlayHide();
        }
    }

    private void OnDisable()
    {
        SetHiddenImmediate();
    }

    private void RefreshInfo()
    {
        if (stageIdText != null)
        {
            stageIdText.text = stageId;
        }

        if (progressText == null) return;

        if (ProgressManager.Instance == null)
        {
            progressText.text = "- / -";
            return;
        }

        var data = ProgressManager.Instance.GetStageProgress(stageId);

        if (data == null)
        {
            progressText.text = $"<color=#AAAAAA>0 / ?</color>";
            return;
        }

        progressText.text = $"{data.bestCollectedCount} / {data.totalItemCount}";
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

        if (!isActiveAndEnabled)
        {
            return;
        }

        showHideCo = StartCoroutine(CoFadeMove(0f, 1f, hiddenPos, shownPos));
    }

    private void PlayHide()
    {
        if (showHideCo != null)
        {
            StopCoroutine(showHideCo);
            showHideCo = null;
        }

        if (!gameObject.activeInHierarchy || !enabled)
        {
            SetHiddenImmediate();
            return;
        }

        if (infoRoot == null || canvasGroup == null || infoPanelRect == null)
        {
            SetHiddenImmediate();
            return;
        }

        showHideCo = StartCoroutine(CoHide());
    }

    private IEnumerator CoHide()
    {
        yield return CoFadeMove(1f, 0f, shownPos, hiddenPos);

        if (infoRoot != null)
        {
            infoRoot.SetActive(false);
        }
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
