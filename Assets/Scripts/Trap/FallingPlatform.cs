using UnityEngine;
using DG.Tweening;

public class FallingPlatform : MonoBehaviour
{
    // ===== Inspector設定 =====
    [Header("Fall Settings")]
    [SerializeField] private float fallDelay = 0.5f; // 落下するまでの遅延時間
    [SerializeField] private float fallDistance = 5f; // 落下する距離
    [SerializeField] private float fallDuration = 1f; // 落下にかかる時間
    [SerializeField] private float respawnDelay = 2f; // 再出現までの遅延時間

    [Header("Sink Settings")]
    [SerializeField] private float sinkDistance = 0.2f; // 沈む量
    [SerializeField] private float sinkDuration = 0.15f; // 沈む時間

    // ===== 内部管理 =====
    private Vector3 startPos;

    private Tween floatTween;
    private Tween sinkTween;
    private Tween fallTween;

    private bool isFalling;

    // ===== コンポーネント =====
    private Animator animator;


    // ===== Unityイベント =====
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        startPos = transform.position;
        StartFloatTween();
    }

    private void OnDisable()
    {
        CleanupTweensAndInvoke();
    }

    private void OnDestroy()
    {
        CleanupTweensAndInvoke();
    }

    // ===== 衝突イベント =====
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y < -0.5f)
        {
            animator.enabled = false; // アニメーションを停止
            isFalling = true;

            floatTween?.Kill();

            Sink();
        }
    }

    // ===== プライベートメソッド =====

    // 沈む処理
    private void Sink()
    {
        sinkTween?.Kill();

        sinkTween = transform.DOMoveY(transform.position.y - sinkDistance, sinkDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                Invoke(nameof(Fall), fallDelay);
            });
    }

    // 落下処理
    private void Fall()
    {
        fallTween?.Kill();

        fallTween = transform.DOMoveY(transform.position.y - fallDistance, fallDuration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject)
            .OnComplete(() => 
            {
                Invoke(nameof(Respawn), respawnDelay);
            });
    }

    // 再出現処理
    private void Respawn()
    {
        fallTween?.Kill();
        sinkTween?.Kill();

        transform.position = startPos;

        animator.enabled = true; // アニメーションを再開
        isFalling = false;

        StartFloatTween();
    }

    private void StartFloatTween()
    {
        floatTween?.Kill();

        floatTween = transform.DOMoveY(startPos.y + 0.3f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject);
    }

    private void CleanupTweensAndInvoke()
    {
        CancelInvoke();

        floatTween?.Kill();
        sinkTween?.Kill();
        fallTween?.Kill();
    }
}
