using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialEntrance : MonoBehaviour
{
    [Header("Tutorial Info")]
    [SerializeField] private string actionName;
    [SerializeField] private string buttonText;

    [Header("UI")]
    [SerializeField] private GameObject infoRoot;
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private TextMeshProUGUI buttonTextUI;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.2f;

    private Coroutine fadeCo;

    private void Awake()
    {
        if (canvasGroup == null && infoRoot != null)
        {
            canvasGroup = infoRoot.GetComponent<CanvasGroup>();
        }

        RefreshInfo();
        SetHiddenImmediate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm == null) return;

        RefreshInfo();
        PlayShow();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm == null) return;

        PlayHide();
    }

    private void OnDisable()
    {
        SetHiddenImmediate();
    }

    private void RefreshInfo()
    {
        if (actionNameText != null)
        {
            actionNameText.text = actionName;
        }

        if (buttonTextUI != null)
        {
            buttonTextUI.text = buttonText;
        }
    }

    private void PlayShow()
    {
        if (fadeCo != null)
        {
            StopCoroutine(fadeCo);
            fadeCo = null;
        }

        if (infoRoot != null)
        {
            infoRoot.SetActive(true);
        }

        if (!isActiveAndEnabled) return;

        fadeCo = StartCoroutine(CoFade(0f, 1f));
    }

    private void PlayHide()
    {
        if (fadeCo != null)
        {
            StopCoroutine(fadeCo);
            fadeCo = null;
        }

        if (!gameObject.activeInHierarchy || !enabled)
        {
            SetHiddenImmediate();
            return;
        }

        if (infoRoot == null || canvasGroup == null)
        {
            SetHiddenImmediate();
            return;
        }

        fadeCo = StartCoroutine(CoHide());
    }

    private IEnumerator CoHide()
    {
        yield return CoFade(1f, 0f);

        if (infoRoot != null)
        {
            infoRoot.SetActive(false);
        }
    }

    private IEnumerator CoFade(float fromAlpha, float toAlpha)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float time = 0f;
        canvasGroup.alpha = fromAlpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            float eased = t * t * (3f - 2f * t);

            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    private void SetHiddenImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (infoRoot != null)
        {
            infoRoot.SetActive(false);
        }
    }
}
