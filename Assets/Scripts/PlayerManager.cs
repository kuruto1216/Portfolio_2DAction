using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // ===== 変数 =====

    [SerializeField] GameManager gameManager;   //　GameManager取得

    //　移動用変数
    [Header("移動速度")]
    [SerializeField] float speed = 5f;  //　移動速度（Inspectorから調整）
    float move;                         //　横移動（-1 ～ 1）
    bool isFacingRight = true;          //　右向き判定
    Rigidbody2D rb;                     //　物理操作用

    //　ジャンプ用変数
    [Header("ジャンプ力")]
    [SerializeField] float jumpPower = 8f;  //　ジャンプ力
    bool isGrounded;                        //　地面にいるか
    bool wasGrounded;                       //  空中から地面戻り判定用
    bool canControl = false;                    //　操作可能状態か

    //　壁ジャンプ用変数
    [Header("壁ジャンプ用設定")]
    [SerializeField] float wallJumpPower = 8f;      //　壁ジャンプ力
    [SerializeField] float wallJumpHorizontal = 5f; //　壁ジャンプ横方向力
    [SerializeField] float wallSlideSpeed = 1f;     //　壁張り付き中の最大落下速度
    bool isOnWall;                          //　壁接触判定
    bool isOnLeftWall;                      //　左壁接触判定
    bool isOnRightWall;                     //　右壁接触判定

    //　壁ジャンプ中ロック変数
    [SerializeField] float wallJumpControlLockTime = 0.2f;  //　壁ジャンプ後の操作ロック時間
    [SerializeField] float facingLockTime = 0.2f;           //　壁ジャンプ後の向き固定時間
    float wallJumpControlLockTimer;     //　操作ロックタイマー
    float facingLockTimer;              //　向き固定タイマー
    bool isFacingLocked;                //　向き固定判定

    //　2段ジャンプ用変数
    [Header("2段ジャンプ用設定")]
    [SerializeField] int maxJumpCount = 2;  //　最大ジャンプ可能数
    int jumpCount;                          //  ジャンプ回数カウンター
    [SerializeField] float groundIgnoreTime = 0.1f; //　地面判定無視時間
    float groundIgnoreTimer;                        //　無視時間タイマー

    //　ジャンプバッファ
    [Header("ジャンプ入力猶予時間")]
    [SerializeField] float jumpBufferTime = 0.1f;   //　入力猶予時間
    float jumpBufferCounter;                        //　猶予時間カウンター

    //　レイヤー取得
    [Header("レイヤー設定")]
    [SerializeField] LayerMask groundLayer; //　地面判定
    [SerializeField] LayerMask wallLayer;   //　壁判定

    //　アニメーション
    Animator animator;

    // ===== Unityイベント =====
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Appear();   //　Player登場処理
    }

    private void Update()
    {
        if (!canControl) return;
        
        UpdateGroundState();        //　地面判定の更新
        UpdateWallState();          //　壁判定の更新
        UpdateJumpBuffer();         //　ジャンプ入力バッファ更新

        //　見た目・アニメ更新
        UpdateFacingLock();
        UpdateFacing();
        UpdateAnimator();
    }
    private void FixedUpdate()
    {
        if (!canControl) return;

        //　横移動処理（壁ジャンプ中は操作ロック）
        if (wallJumpControlLockTimer > 0)
        {
            wallJumpControlLockTimer -= Time.fixedDeltaTime;
        }
        else
        {
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }

        ApplyWallSlide();       //　壁張り付き処理
        TryJump();              //　ジャンプ入力時処理               
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            Debug.Log("ゲームオーバー");
            Death();
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


    // ===== 自作メソッド部分 =====

    //　Player登場時設定
    void Appear()
    {
        DisablePlayerControl();
        animator.SetTrigger("appear");
    }

    //　Appearアニメイベント(操作可能切り替え)
    void OnAppearFinished()
    {
        rb.simulated = true;
        canControl = true;
    }

    //　プレイヤー機能停止
    void DisablePlayerControl()
    {
        canControl = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    //　アニメーションアップデート
    void UpdateAnimator()
    {
        animator.SetFloat("speed", Mathf.Abs(move));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetInteger("jumpCount", jumpCount);

        bool isWallSliding = isOnWall && !isGrounded && rb.linearVelocity.y <= 0f;

        animator.SetBool("isWallSliding", isWallSliding);
    }

    //　moveの入力値に応じてPlayerの向き切り替え
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

    //　向きロックタイマー更新
    void UpdateFacingLock()
    {
        if (!isFacingLocked) return;

        facingLockTimer -= Time.deltaTime;
        if (facingLockTimer <= 0)
        {
            isFacingLocked = false;
        }
    }

    //　Playerの向き反転
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    //　ジャンプ処理本体
    void Jump()
    {
        groundIgnoreTimer = groundIgnoreTime;   //　地面判定無視時間セット

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    //　ジャンプ入力バッファ更新
    void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    //　ジャンプ試行
    void TryJump()
    {
        if (jumpBufferCounter <= 0) return;

        //　壁ジャンプ処理
        if (isOnWall && !isGrounded)
        {
            jumpBufferCounter = 0;
            WallJump();
            return;
        }

        //　通常ジャンプ・2段ジャンプ処理
        if (jumpCount < maxJumpCount)
        {
            jumpCount++;
            jumpBufferCounter = 0;
            Jump();
        }
    }

    //　壁張り付き処理
    void ApplyWallSlide()
    {
        if (!isOnWall) return;
        if (isGrounded) return;

        if (rb.linearVelocity.y > 0) return;

        //　落下速度制限
        if (rb.linearVelocity.y < -wallSlideSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
    }

    //　壁ジャンプ
    void WallJump()
    {
        //　壁ジャンプ時向き調整
        float direction = isOnLeftWall ? 1f : -1f;
        if (direction > 0 && !isFacingRight) Flip();
        if (direction < 0 && isFacingRight) Flip();

        //　向き固定設定
        isFacingLocked = true;
        facingLockTimer = facingLockTime;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            new Vector2(direction * wallJumpHorizontal, wallJumpPower),
            ForceMode2D.Impulse
        );

        jumpCount = 1;
        Debug.Log("壁ジャンプ");

        wallJumpControlLockTimer = wallJumpControlLockTime; //　操作ロックタイマーセット
    }

    //　死亡処理
    void Death()
    {
        if (!canControl) return;

        DisablePlayerControl();
        animator.SetTrigger("die");
        gameManager.GameOver();
    }

    //　地面判定
    bool IsGrounded()
    {

        Vector3 leftStartPoint = transform.position - Vector3.right * 0.2f;
        Vector3 rightStartPoint = transform.position + Vector3.right * 0.2f;
        Vector3 endPoint = transform.position - Vector3.up * 0.1f;
        Debug.DrawLine(leftStartPoint, endPoint);
        Debug.DrawLine(rightStartPoint, endPoint);

        return Physics2D.Linecast(leftStartPoint, endPoint, groundLayer)
            || Physics2D.Linecast(rightStartPoint, endPoint, groundLayer);
    }

    //　地面状態更新
    void UpdateGroundState()
    {
        //　ジャンプ直後の地面判定回避のため、groundIgnoreTimer中は強制的にisGrounded = false;
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
            Debug.Log("着地");
        }
        wasGrounded = isGrounded;
    }

    //　左壁判定
    bool CheckLeftWall()
    {
        Vector3 upper = transform.position + Vector3.up * 0.4f;
        Vector3 lower = transform.position + Vector3.up * 0.1f;

        Vector3 upperEnd = upper - Vector3.right * 0.4f;
        Vector3 lowerEnd = lower - Vector3.right * 0.4f;

        Debug.DrawLine(upper, upperEnd, Color.red);
        Debug.DrawLine(lower, lowerEnd, Color.red);

        return Physics2D.Linecast(upper, upperEnd, wallLayer)
            || Physics2D.Linecast(lower, lowerEnd, wallLayer);
    }

    //　右壁判定
    bool CheckRightWall()
    {
        Vector3 upper = transform.position + Vector3.up * 0.4f;
        Vector3 lower = transform.position + Vector3.up * 0.1f;

        Vector3 upperEnd = upper + Vector3.right * 0.4f;
        Vector3 lowerEnd = lower + Vector3.right * 0.4f;

        Debug.DrawLine(upper, upperEnd, Color.red);
        Debug.DrawLine(lower, lowerEnd, Color.red);

        return Physics2D.Linecast(upper, upperEnd, wallLayer)
            || Physics2D.Linecast(lower, lowerEnd, wallLayer);
    }

    //　壁状態更新
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
        //Debug.Log("LeftWall: " + isOnLeftWall + " RightWall: " + isOnRightWall);
        isOnWall = isOnLeftWall || isOnRightWall;
    }
}
