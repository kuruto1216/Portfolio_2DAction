using UnityEngine;
using DG.Tweening;

public class FallingPlatform : MonoBehaviour
{
    // ===== Inspector設定 =====
    public float fallDelay = 0.5f; // 落下するまでの遅延時間
    public float fallDistance = 5f; // 落下する距離
    public float fallDuration = 1f; // 落下にかかる時間
    public float respawnDelay = 2f; // 再出現までの遅延時間

    public float sinkDistance = 0.2f; // 沈む量
    public float sinkDuration = 0.15f; // 沈む時間

    // ===== 内部管理 =====
    Vector3 startPos;

    Tween floatTween;
    Tween fallTween;

    bool isFalling;

    // ===== コンポーネント =====
    Animator animator;


    // ===== Unityイベント =====
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        startPos = transform.position;

        floatTween = transform.DOMoveY(startPos.y + 0.3f, 1f)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetEase(Ease.InOutSine);
    }

    // ===== 衝突イベント =====
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                animator.enabled = false; // アニメーションを停止
                isFalling = true;

                floatTween?.Kill();

                Sink();
            }
        }
    }

    // ===== プライベートメソッド =====

    // 沈む処理
    void Sink()
    {
        transform.DOMoveY(transform.position.y - sinkDistance, sinkDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        Invoke(nameof(Fall), fallDelay);
                    });
    }

    // 落下処理
    void Fall()
    {
        fallTween = transform.DOMoveY(transform.position.y - fallDistance, fallDuration)
                             .SetEase(Ease.InQuad)
                             .OnComplete(() => 
                             {
                                 Invoke(nameof(Respawn), respawnDelay);
                             });
    }

    // 再出現処理
    void Respawn()
    {
        fallTween?.Kill();

        transform.position = startPos;

        floatTween = transform.DOMoveY(startPos.y + 0.3f, 1f)
                                .SetLoops(-1, LoopType.Yoyo)
                                .SetEase(Ease.InOutSine);

        animator.enabled = true; // アニメーションを再開
        isFalling = false;
    }
}
