using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.3f;

    private bool isTransitioning = false;

    [Header("Diamond")]
    [SerializeField] private RectTransform diamondRect;
    [SerializeField] private float diamondCloseDuration = 0.35f;
    [SerializeField] private float diamondOpenDuration = 0.35f;
    [SerializeField] private float diamondMaxScale = 20f;

    [SerializeField] private bool useDiamond = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        if (useDiamond)
        {
            yield return DiamondTransition(0f, diamondMaxScale, diamondCloseDuration);
        }
        else
        {
            yield return Fade(0f, 1f, fadeOutDuration);
        }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        if (useDiamond)
        {
            yield return DiamondTransition(diamondMaxScale, 0f, diamondOpenDuration);
        }
        else
        {
            yield return Fade(1f, 0f, fadeInDuration);
        }

            isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;

        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;
        fadeImage.raycastTarget = true;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;

            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;

        fadeImage.raycastTarget = endAlpha > 0f;
    }

    private IEnumerator DiamondTransition(float startScale, float endScale, float duration)
    {
        float time = 0f;

        diamondRect.gameObject.SetActive(true);
        diamondRect.localScale = Vector3.one * startScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);

            diamondRect.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            yield return null;
        }

        diamondRect.localScale = Vector3.one * endScale;

        if (endScale <= 0f)
        {
            diamondRect.gameObject.SetActive(false);
        }
    }
}
