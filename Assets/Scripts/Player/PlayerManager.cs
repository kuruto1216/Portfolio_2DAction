using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // ===== 列挙型 =====
    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        DoubleJump,
        Fall,
        WallSlide,
        Dead
    }

    PlayerState currentState;

    // ===== 変数 =====

    [SerializeField] GameManager gameManager;   // GameManager取得

    // 移動用変数
    [Header("移動速度")]
    [SerializeField] float speed = 5f;  // 移動速度（Inspectorから調整）
    float move;                         // 横移動（-1 ～ 1）
    bool isFacingRight = true;          // 右向き判定
    Rigidbody2D rb;                     // 物理操作用

    // ジャンプ用変数
    [Header("ジャンプ力")]
    [SerializeField] float jumpPower = 8f;  // ジャンプ力
    bool isGrounded;                        // 地面にいるか
    bool wasGrounded;                       // 空中から地面戻り判定用
    bool canControl = false;                    // 操作可能状態か

    // 壁ジャンプ用変数
    [Header("壁ジャンプ用設定")]
    [SerializeField] float wallJumpPower = 8f;      // 壁ジャンプ力
    [SerializeField] float wallJumpHorizontal = 5f; // 壁ジャンプ横方向力
    [SerializeField] float wallSlideSpeed = 1f;     // 壁張り付き中の最大落下速度
    bool isOnWall;                          // 壁接触判定
    bool isOnLeftWall;                      // 左壁接触判定
    bool isOnRightWall;                     // 右壁接触判定

    // 壁ジャンプ中ロック変数
    [SerializeField] float wallJumpControlLockTime = 0.2f;  // 壁ジャンプ後の操作ロック時間
    [SerializeField] float facingLockTime = 0.2f;           // 壁ジャンプ後の向き固定時間
    float wallJumpControlLockTimer;     // 操作ロックタイマー
    float facingLockTimer;              // 向き固定タイマー
    bool isFacingLocked;                // 向き固定判定

    // 2段ジャンプ用変数
    [Header("2段ジャンプ用設定")]
    [SerializeField] int maxJumpCount = 2;  // 最大ジャンプ可能数
    int jumpCount;                          // ジャンプ回数カウンター
    [SerializeField] float groundIgnoreTime = 0.1f; // 地面判定無視時間
    float groundIgnoreTimer;                        // 無視時間タイマー

    // ジャンプバッファ
    [Header("ジャンプ入力猶予時間")]
    [SerializeField] float jumpBufferTime = 0.1f;   // 入力猶予時間
    float jumpBufferCounter;                        // 猶予時間カウンター

    // 動的重力調整用変数
    [Header("重力調整用設定")]
    [SerializeField] float riseGravity = 2f;    // 上昇中の重力倍率
    [SerializeField] float fallGravity = 3f;    // 下降中の重力倍率

    // レイヤー取得
    [Header("レイヤー設定")]
    [SerializeField] LayerMask groundLayer; // 地面判定
    [SerializeField] LayerMask wallLayer;   // 壁判定

    // VisualのTransform取得（向き反転用）
    [SerializeField] Transform visual;

    // アニメーション
    Animator animator;

    MovingPlatform currentPlatform;

    // ===== Unityイベント =====
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        DisablePlayerControl();
    }

    private void Update()
    {
        if (!canControl) return;

        UpdateGroundState();        // 地面判定の更新
        UpdateWallState();          // 壁判定の更新
        UpdateJumpBuffer();         // ジャンプ入力バッファ更新

        CheckLeftWall();
        CheckRightWall();

        UpdateState();            // Player状態更新
        UpdateByState();          // 状態に応じた更新処理

        // 見た目・アニメ更新
        UpdateFacingLock();
        UpdateFacing();
        UpdateAnimator();

        //Debug.Log(isGrounded);
    }

    private void FixedUpdate()
    {
        if (!canControl) return;

        // 横移動処理（壁ジャンプ中は操作ロック）
        if (wallJumpControlLockTimer > 0)
        {
            wallJumpControlLockTimer -= Time.fixedDeltaTime;
        }
        else
        {
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }

        // 壁張り付き落下速度制限
        if (currentState == PlayerState.WallSlide)
        {
            ApplyWallSlide();
        }

        TryJump();              // ジャンプ入力時処理
        ApplyDynamicGravity();  // 動的重力調整

        // 移動床の移動量を加算
        if (isGrounded && currentPlatform != null)
        {
            rb.position += currentPlatform.Delta;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            Debug.Log("ゲームオーバー");
            RequestDeath();
        }
        if (collision.CompareTag("Finish"))
        {
            Debug.Log("ゲームクリア");
            DisablePlayerControl();
            gameManager.GameClear();
        }
        if (collision.CompareTag("Item"))
        {
            collision.gameObject.GetComponent<ItemManager>().GetItem();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var platform = collision.collider.GetComponent<MovingPlatform>();

        if (platform != null)
        {
            currentPlatform = platform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var platform = collision.collider.GetComponent<MovingPlatform>();

        if (platform != null && currentPlatform == platform)
        {
            currentPlatform = null;
        }
    }

    // ===== InputSystemイベント部分 =====

    public void OnMove(InputValue value)
    {
        if (!canControl) return;

        move = value.Get<float>();
    }

    public void OnJump(InputValue value)
    {
        if (!canControl) return;

        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }


    // ===== 自作メソッド =====

    // ---状態管理関連メソッド---

    // Player状態更新
    void UpdateState()
    {
        if (currentState == PlayerState.Dead)
        {
            return;
        }

        if (isGrounded)
        {
            if (Mathf.Abs(move) > 0.01f)
            {
                ChangeState(PlayerState.Run);
            }
            else
            {
                ChangeState(PlayerState.Idle);
            }
            return;
        }

        if (isOnWall && rb.linearVelocity.y <= 0f)
        {
            ChangeState(PlayerState.WallSlide);
            return;
        }

        if (rb.linearVelocity.y > 0.1f)
        {
            if (jumpCount >= maxJumpCount)
            {
                ChangeState(PlayerState.DoubleJump);
            }
            else
            {
                ChangeState(PlayerState.Jump);
            }
        }
        else
        {
            ChangeState(PlayerState.Fall);
        }
    }

    // 状態変更時処理
    void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;

        PlayerState previousState = currentState;   // 前の状態保存(使用しないが将来の拡張用に)
        currentState = newState;
        Debug.Log("State: " + currentState);

        switch (newState)
        {
            case PlayerState.WallSlide:
                jumpCount = 0;  // 壁を地面とみなしてジャンプ回数リセット
                break;
        }
    }

    // 状態に応じた更新処理(空のメソッド群は将来の拡張用)
    void UpdateByState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;

            case PlayerState.Run:
                UpdateRun();
                break;

            case PlayerState.Jump:
            case PlayerState.Fall:
                UpdateAir();
                break;

            case PlayerState.WallSlide:
                UpdateWallSlide();
                break;

            case PlayerState.Dead:

                break;
        }
    }

    // ---状態別更新メソッド群---
    void UpdateIdle()
    {
    }
    void UpdateRun()
    {
    }
    void UpdateAir()
    {
    }
    void UpdateWallSlide()
    {
    }

    // ---動作メソッド群---

    // ジャンプ試行
    void TryJump()
    {
        if (jumpBufferCounter <= 0) return;

        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
                GroundJump();
                break;

            case PlayerState.Jump:
            case PlayerState.Fall:
                AirJump();
                break;

            case PlayerState.WallSlide:
                WallJump();
                break;
        }
    }

    void GroundJump()
    {
        jumpCount = 1;
        jumpBufferCounter = 0;
        Jump();
    }

    // 2段ジャンプ
    void AirJump()
    {
        if (jumpCount >= maxJumpCount) return;

        jumpCount++;
        jumpBufferCounter = 0;

        Jump();
    }

    // 壁ジャンプ
    void WallJump()
    {
        jumpBufferCounter = 0;

        // 壁ジャンプ時向き調整
        float direction = isOnLeftWall ? 1f : -1f;

        if (direction > 0 && !isFacingRight) Flip();
        if (direction < 0 && isFacingRight) Flip();

        // 向き固定設定
        isFacingLocked = true;
        facingLockTimer = facingLockTime;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            new Vector2(direction * wallJumpHorizontal, wallJumpPower),
            ForceMode2D.Impulse
        );

        jumpCount = 1;
        //Debug.Log("壁ジャンプ");

        wallJumpControlLockTimer = wallJumpControlLockTime; // 操作ロックタイマーセット
    }

    // ジャンプ処理本体
    void Jump()
    {
        groundIgnoreTimer = groundIgnoreTime;   // 地面判定無視時間セット

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    // 壁張り付き処理
    void ApplyWallSlide()
    {
        if (!isOnWall) return;
        if (isGrounded) return;

        if (rb.linearVelocity.y > 0) return;

        // 落下速度制限
        if (rb.linearVelocity.y < -wallSlideSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
    }

    // ジャンプ入力バッファ更新
    void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    // ---判定更新メソッド群---

    // 地面状態更新
    void UpdateGroundState()
    {
        // ジャンプ直後の地面判定回避のため、groundIgnoreTimer中は強制的にisGrounded = false;
        if (groundIgnoreTimer > 0)
        {
            groundIgnoreTimer -= Time.deltaTime;
            isGrounded = false;
        }
        else
        {
            isGrounded = IsGrounded();
        }

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
        wasGrounded = isGrounded;
    }

    // 地面判定
    bool IsGrounded()
    {

        Vector3 leftStartPoint = transform.position - Vector3.right * 0.3f;
        Vector3 rightStartPoint = transform.position + Vector3.right * 0.3f;
        Vector3 endPoint = transform.position - Vector3.up * 0.1f;
        Debug.DrawLine(leftStartPoint, endPoint);
        Debug.DrawLine(rightStartPoint, endPoint);

        return Physics2D.Linecast(leftStartPoint, endPoint, groundLayer)
            || Physics2D.Linecast(rightStartPoint, endPoint, groundLayer);
    }

    // 壁状態更新
    void UpdateWallState()
    {
        if (isGrounded)
        {
            isOnWall = false;
            isOnLeftWall = false;
            isOnRightWall = false;
            return;
        }

        isOnLeftWall = CheckLeftWall();
        isOnRightWall = CheckRightWall();

        isOnWall = isOnLeftWall || isOnRightWall;
    }

    // 左壁判定
    bool CheckLeftWall()
    {
        Vector3 upper = transform.position + Vector3.up * 1.0f;
        Vector3 lower = transform.position + Vector3.up * 0.4f;

        Vector3 upperEnd = upper - Vector3.right * 0.6f;
        Vector3 lowerEnd = lower - Vector3.right * 0.6f;

        Debug.DrawLine(upper, upperEnd, Color.red);
        Debug.DrawLine(lower, lowerEnd, Color.red);

        return Physics2D.Linecast(upper, upperEnd, wallLayer)
            || Physics2D.Linecast(lower, lowerEnd, wallLayer);
    }

    // 右壁判定
    bool CheckRightWall()
    {
        Vector3 upper = transform.position + Vector3.up * 1.0f;
        Vector3 lower = transform.position + Vector3.up * 0.4f;

        Vector3 upperEnd = upper + Vector3.right * 0.6f;
        Vector3 lowerEnd = lower + Vector3.right * 0.6f;

        Debug.DrawLine(upper, upperEnd, Color.red);
        Debug.DrawLine(lower, lowerEnd, Color.red);

        return Physics2D.Linecast(upper, upperEnd, wallLayer)
            || Physics2D.Linecast(lower, lowerEnd, wallLayer);
    }

    // ---見た目・アニメ関連メソッド群---

    // アニメーションアップデート
    void UpdateAnimator()
    {
        animator.SetInteger("State", (int)currentState);
    }

    // moveの入力値に応じてPlayerの向き切り替え
    void UpdateFacing()
    {
        if (isFacingLocked) return;

        if (move > 0.01f && !isFacingRight)
        {
            Flip();
        }
        else if (move < -0.01f && isFacingRight)
        {
            Flip();
        }
    }

    // 向きロックタイマー更新
    void UpdateFacingLock()
    {
        if (!isFacingLocked) return;

        facingLockTimer -= Time.deltaTime;
        if (facingLockTimer <= 0)
        {
            isFacingLocked = false;
        }
    }

    // Playerの向き反転
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = visual.localScale;
        scale.x *= -1;
        visual.localScale = scale;
    }

    // ---特殊処理メソッド群---

    // 動的重力調整
    void ApplyDynamicGravity()
    {
        if (isGrounded)
        {
            rb.gravityScale = 2f; // 地面にいるときの重力
            return;
        }

        if (rb.linearVelocity.y > 0.1f)
        {
            rb.gravityScale = riseGravity; // 上昇中の重力
        }
        else
        {
            rb.gravityScale = fallGravity; // 下降中の重力（落下を速くする）
        }
    }

    public void Bounce(float bouncePower)
    {
        if (!canControl) return;
        if (currentState == PlayerState.Dead) return;

        groundIgnoreTimer = groundIgnoreTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * bouncePower, ForceMode2D.Impulse);

        // 空中扱いでジャンプ回数消費
        jumpCount = Mathf.Max(jumpCount, 1);
    }

    // 死亡状態リクエスト
    void RequestDeath()
    {
        if (currentState == PlayerState.Dead) return;

        ChangeState(PlayerState.Dead);
    }

    // Deathアニメイベント(死亡処理)
    public void Death()
    {
        if (!canControl) return;

        DisablePlayerControl();
        gameManager.GameOver();
    }

    // プレイヤー機能停止
    void DisablePlayerControl()
    {
        canControl = false;                 //　ロジック停止
        move = 0f;                          //　入力停止
        rb.linearVelocity = Vector2.zero;   //　速度停止
        rb.angularVelocity = 0f;            //　回転停止
        rb.simulated = false;               //　物理停止
    }

    // Appearアニメイベント(操作可能切り替え)
    public void OnAppearFinished()
    {
        rb.simulated = true;
        canControl = true;
        ChangeState(PlayerState.Idle);
    }

    // リスポーン処理
    public void RespawnTo(Vector3 pos)
    {
        DisablePlayerControl();

        transform.position = pos;   // 保存位置に移動

        // 状態・アニメの強制リセット
        ChangeState(PlayerState.Idle);
        animator.SetInteger("State", (int)PlayerState.Idle);

        // Appearアニメ再生
        animator.ResetTrigger("Appear");
        animator.SetTrigger("Appear");
    }
}
