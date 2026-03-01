using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class MovingPlatform : MonoBehaviour, IPlatformDelta
{
    [Header("Move Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Move Settings")]
    [SerializeField] private float speed = 1f;

    public Vector2 Delta { get; private set; }

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 endPos;

    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (startPoint != null) startPos = startPoint.position;
        if (endPoint != null) endPos = endPoint.position;

        // 初期位置をスタートポイントに設定
        rb.position = startPos;
        timer = 0f;
        Delta = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (startPoint == null || endPoint == null) 
        {
            Delta = Vector2.zero;
            return;
        }

        // 毎フレーム、スタートとエンドの位置を更新（エディタ上で動かせるように）
        startPos = startPoint.position;
        endPos = endPoint.position;

        timer += Time.fixedDeltaTime;   // 物理の時間でタイマーを進める

        // 0～1の範囲で往復する値を生成
        float t = Mathf.PingPong(timer * speed, 1f);

        Vector2 current = rb.position;                          // 現在の位置を保存
        Vector2 nextPos = Vector2.Lerp(startPos, endPos, t);    // 次の位置を計算

        rb.MovePosition(nextPos);   // 移動

        Delta = nextPos - current;  // 移動量の計算(プレイヤー側に加算するため)
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (startPoint == null || endPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
    }
#endif
}
