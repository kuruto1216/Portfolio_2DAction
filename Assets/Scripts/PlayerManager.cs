using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    // ===== 変数 =====

    [SerializeField] GameManager gameManager;   //　GameManager取得
    [Header("移動速度")]
    [SerializeField] float speed = 5f;  //　移動速度（Inspectorから調整）
    float move;                         //　横移動（-1 ～ 1）
    bool isFacingRight = true;          //　右向き判定
    Rigidbody2D rb;                     //　物理操作用

    [Tooltip("ジャンプ力")]
    [SerializeField] float jumpPower = 8f;  //　ジャンプ力
    [SerializeField] LayerMask groundLayer; //　地面判定
    bool isGrounded;                        //　地面にいるか
    bool wasGrounded;                       //  空中から地面戻り判定用
    bool canControl = false;                    //　操作可能状態か

    //　2段ジャンプ用変数
    [SerializeField] int maxJumpCount = 2;  //　最大ジャンプ可能数
    int jumpCount;                          //  回数カウンター
    [SerializeField] float groundIgnoreTime = 0.1f; //　地面判定無視時間
    float groundIgnoreTimer;                        //　無視時間タイマー

    //　ジャンプバッファ
    [SerializeField] float jumpBufferTime = 0.1f;   //　入力猶予時間
    float jumpBufferCounter;                        //　猶予時間カウンター

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
        Appear();
    }

    private void Update()
    {
        if (!canControl) return;

        
        UpdateGroundState();        //　ジャンプ後地面判定の更新
        UpdateJumpBuffer();         //　ジャンプ入力バッファ更新

        //　見た目・アニメ更新
        UpdateFacing();
        UpdateAnimator();
    }
    private void FixedUpdate()
    {
        if (!canControl) return;

        //　横移動
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        //　ジャンプ入力時処理
        TryJump();
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
    }

    //　moveの入力値に応じてPlayerの向き切り替え
    void UpdateFacing()
    {
        if (move > 0.01f && !isFacingRight)
        {
            Flip();
        }
        else if (move < -0.01f && isFacingRight)
        {
            Flip();
        }
    }

    //　向き反転
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    //　ジャンプ
    void Jump()
    {
        groundIgnoreTimer = groundIgnoreTime;   //　地面判定無視時間セット

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    //　ジャンプバッファアップデート
    void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    //　ジャンプ処理
    void TryJump()
    {
        if (jumpBufferCounter > 0 && jumpCount < maxJumpCount)
        {
            jumpCount++;
            jumpBufferCounter = 0;
            Jump();
            Debug.Log(jumpCount);
        }
    }

    //　プレイヤー死亡時
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

    //　地面更新
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
}
