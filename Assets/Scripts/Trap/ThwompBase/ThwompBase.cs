using UnityEngine;

public class ThwompBase : MonoBehaviour
{
    public enum ThwompState { Idle, Blink, Move, Hit, Stun, Return }
    public enum MoveAxis { Down, Left, Right }

    [Header("Move Setting")]
    [SerializeField] MoveAxis axis = MoveAxis.Down;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float returnSpeed = 6f;

    [Header("Timing")]
    [SerializeField] float blinkTime = 0.3f;
    [SerializeField] float stunTime = 0.5f;
    [SerializeField] float hitTime = 0.15f;

    [Header("Detection")]
    [SerializeField] Transform player;
    [SerializeField] Vector2 detectBoxSize = new Vector2(3f, 2f);
    [SerializeField] LayerMask hitLayer;    // 壁/地形

    [Header("Raycast")]
    [SerializeField] float hitRayDistance = 0.2f;

    [Header("Animation")]
    [SerializeField] Animator animator;

    ThwompState state = ThwompState.Idle;
    Rigidbody2D rb;
    Vector3 startPos;
    float timer;

    Vector2 MoveDir
    {
        get
        {
            return axis switch
            {
                MoveAxis.Down => Vector2.down,
                MoveAxis.Left => Vector2.left,
                MoveAxis.Right => Vector2.right,
                _ => Vector2.down
            };
        }
    }

    // ===== Unityイベント =====
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        switch (state)
        {
            case ThwompState.Idle:
                if (IsPlayerDetected())
                {
                    ChangeState(ThwompState.Blink);
                    timer = blinkTime;
                }
                break;

            case ThwompState.Blink:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    ChangeState(ThwompState.Move);
                }
                break;

            case ThwompState.Hit:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    ChangeState(ThwompState.Stun);
                    timer = stunTime;
                }
                break;

            case ThwompState.Stun:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    ChangeState(ThwompState.Return);
                }
                break;

            case ThwompState.Return:
                Vector2 next = Vector2.MoveTowards(rb.position,
                    (Vector2)startPos, returnSpeed * Time.fixedDeltaTime);
                rb.MovePosition(next);

                // 到達判定
                if ((next - (Vector2)startPos).sqrMagnitude < 0.000001f)
                {
                    rb.MovePosition(startPos);
                    ChangeState(ThwompState.Idle);
                }
                break;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ThwompState.Move:
                // 衝突チェック（進行方向へRaycast）
                if (IsHitAhead())
                {
                    rb.linearVelocity = Vector2.zero;
                    ChangeState(ThwompState.Hit);
                    timer = hitTime;
                    return;
                }

                MoveStep(MoveDir, moveSpeed);
                break;

            case ThwompState.Return:
                Vector2 dir = (startPos - transform.position).normalized;
                MoveStep(dir, returnSpeed);
                break;

            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    void MoveStep(Vector2 dir, float speed)
    {
        Vector2 next = rb.position + dir * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }

    bool IsPlayerDetected()
    {
        if (player == null) return false;

        // DownならY、左右ならXの半分を使って箱を前に出す（ズレ防止）
        float half = (axis == MoveAxis.Down) ? (detectBoxSize.y * 0.5f) : (detectBoxSize.x * 0.5f);
        Vector2 center = (Vector2)transform.position + MoveDir * half;

        // 箱の中のColliderを全部取る
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, detectBoxSize, 0f);

        foreach (var h in hits)
        {
            if (h == null) continue;

            if (h.CompareTag("Player")) return true;
        }

        return false;
    }

    bool IsHitAhead()
    {
        // Colliderの中心から進行方向へ短くRaycast（必要なら複数本に増やす）
        Vector2 origin = rb.position + MoveDir * 0.05f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, MoveDir, hitRayDistance, hitLayer);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.collider.transform.root == transform) continue;

            return true;
        }
        return false;
    }

    void ChangeState(ThwompState next)
    {
        if (state == next) return;

        state = next;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetInteger("State", (int)state);
        // 方向別Hitを使いたいなら、axisもAnimatorに渡すと便利
        animator.SetInteger("Axis", (int)axis);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 検知ボックス可視化
        Vector2 dir = MoveDir;
        float half = (axis == MoveAxis.Down) ? (detectBoxSize.y * 0.5f) : (detectBoxSize.x * 0.5f);
        Vector2 center = (Vector2)transform.position + dir * half;
        Gizmos.DrawWireCube(center, detectBoxSize);

        // Ray可視化
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * hitRayDistance);
    }
#endif
}
