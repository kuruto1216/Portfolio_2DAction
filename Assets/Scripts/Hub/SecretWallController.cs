using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SecretWallController : MonoBehaviour
{
    [SerializeField] private Tilemap secretWallTilemap;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float targetAlpha = 0.2f;

    private bool hasTriggered;

    private void Awake()
    {
        if (secretWallTilemap == null)
        {
            Debug.LogError("SecretWallTilemap is not assigned!", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSeconds(delay);

        Color startColor = secretWallTilemap.color;
        Color endColor = startColor;
        endColor.a = targetAlpha;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            secretWallTilemap.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        secretWallTilemap.color = endColor;
    }
}
