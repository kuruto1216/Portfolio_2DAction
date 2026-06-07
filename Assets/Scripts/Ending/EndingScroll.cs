using System.Collections;
using UnityEngine;

public class EndingScroll : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private float scrollSpeed = 50f;

    [Header("Stop")]
    [SerializeField] private float stopY = 1000f;
    [SerializeField] private float waitAfterStop = 2f;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "Title";

    private RectTransform rectTransform;
    private bool isStopped;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (isStopped) return;

        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y >= stopY)
        {
            StopScroll();
        }
    }

    private void StopScroll()
    {
        isStopped = true;

        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = stopY;
        rectTransform.anchoredPosition = pos;

        StartCoroutine(ReturnToTitleRoutine());
    }

    private IEnumerator ReturnToTitleRoutine()
    {
        yield return new WaitForSeconds(waitAfterStop);

        TransitionManager.Instance.LoadScene(titleSceneName);
    }
}
