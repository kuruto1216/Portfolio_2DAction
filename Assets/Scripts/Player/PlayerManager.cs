using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        Dash,
        Dead
    }

    private PlayerState currentState;

    // ===== 変数 =====

    [SerializeField] private GameManager gameManager;   // GameManager取得

    // 能力解放デバッグ用
    [Header("Abilities (Debug Toggle)")]
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canDoubleJump = true;
    [SerializeField] private bool canWallJump = true;
    [SerializeField] private bool canWallSlide = true;
    [SerializeField] private bool canDash = true;

    // 移動用変数
    [Header("移動速度")]
    [SerializeField] private float speed = 5f;  // 移動速度（Inspectorから調整）
    private float move;                         // 横移動（-1 ～ 1）
    private bool isFacingRight = true;          // 右向き判定
    private Rigidbody2D rb;                     // 物理操作用

    // ダッシュ用変数
    [Header("ダッシュ用設定")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.10f;
    [SerializeField] private float dashCooldown = 0.25f;
    [SerializeField] private bool allowAirDash = true;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDir;
    private bool airDashUsed;

    // ジャンプ用変数
    [Header("ジャンプ力")]
    [SerializeField] private float jumpPower = 8f;  // ジャンプ力
    [Header("可変ジャンプ設定")]
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    private bool isGrounded;                        // 地面にいるか
    private bool wasGrounded;                       // 空中から地面戻り判定用
    private bool canControl = false;                // 操作可能状態か
    private bool jumpReleased;

    // 壁ジャンプ用変数
    [Header("壁ジャンプ用設定")]
    [SerializeField] private float wallJumpPower = 8f;      // 壁ジャンプ力
    [SerializeField] private float wallJumpHorizontal = 5f; // 壁ジャンプ横方向力
    [SerializeField] private float wallSlideSpeed = 1f;     // 壁張り付き中の最大落下速度
    private bool isOnWall;                          // 壁接触判定
    private bool isOnLeftWall;                      // 左壁接触判定
    private bool isOnRightWall;                     // 右壁接触判定

    // 壁ジャンプ中ロック変数
    [SerializeField] private float wallJumpControlLockTime = 0.2f;  // 壁ジャンプ後の操作ロック時間
    [SerializeField] private float facingLockTime = 0.2f;           // 壁ジャンプ後の向き固定時間
    private float wallJumpControlLockTimer;     // 操作ロックタイマー
    private float facingLockTimer;              // 向き固定タイマー
    private bool isFacingLocked;                // 向き固定判定

    // 2段ジャンプ用変数
    [Header("2段ジャンプ用設定")]
    [SerializeField] private int maxJumpCount = 2;  // 最大ジャンプ可能数
    private int jumpCount;                          // ジャンプ回数カウンター
    [SerializeField] private float groundIgnoreTime = 0.1f; // 地面判定無視時間
    private float groundIgnoreTimer;                        // 無視時間タイマー

    // ジャンプバッファ
    [Header("ジャンプ入力猶予時間")]
    [SerializeField] private float jumpBufferTime = 0.1f;   // 入力猶予時間
    private float jumpBufferCounter;                        // 猶予時間カウンター

    // コヨーテタイム
    [Header("コヨーテタイム設定")]
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimer;

    // 動的重力調整用変数
    [Header("重力調整用設定")]
    [SerializeField] private float riseGravity = 2f;    // 上昇中の重力倍率
    [SerializeField] private float fallGravity = 3f;    // 下降中の重力倍率

    // レイヤー取得
    [Header("レイヤー設定")]
    [SerializeField] private LayerMask groundLayer; // 地面判定
    [SerializeField] private LayerMask wallLayer;   // 壁判定

    // VisualのTransform取得（向き反転用）
    [SerializeField] private Transform visual;

    // アニメーション
    private Animator animator;

    // 移動床用
    private IPlatformDelta currentPlatform;
    private bool isOnDeltaPlatform;

    // OneWay床用
    [Header("OneWay床設定")]
    [SerializeField] private LayerMask oneWayLayer;
    [SerializeField] private float dropThroughTime = 0.2f;  // すり抜け時間
    [SerializeField] private string oneWayLayerName = "OneWay";
    private int playerLayer;
    private int oneWayGroundLayer;

    // 完全停止壁用
    [Header("完全停止壁Layer設定")]
    [SerializeField] private LayerMask stickyWallLayer;
    private bool isOnStickyWall;
    private bool isOnLeftStickyWall;
    private bool isOnRightStickyWall;

    private IPlatformDelta currentStickyWallPlatform;

    // 完全停止壁猶予時間
    [Header("完全停止壁猶予時間")]
    [SerializeField] private float stickyWallGraceTime = 0.05f;
    private float stickyWallGraceTimer;

    // 移動ポータル用
    private PortalEntrance currentPortal;

    // 解放ゲート用
    private GateTrigger currentGate;

    // Arrowギミック用
    [Header("Arrow用設定")]
    [SerializeField] private float arrowControlLockTime = 0.15f;
    [SerializeField] private float arrowJumpCutIgnoreTime = 0.3f;

    private float arrowControlLockTimer;
    private float arrowJumpCutIgnoreTimer;
    private bool isArrowBoosting;

    // 圧死判定用
    [Header("圧死判定設定")]
    [SerializeField] private float crushNormalThreshold = 0.6f;

    private bool pressFromLeft_Rock, pressFromRight_Rock, pressFromUp_Rock, pressFromDown_Rock;
    private bool pressFromLeft_Other, pressFromRight_Other, pressFromUp_Other, pressFromDown_Other;

    // エフェクト用変数
    [Header("エフェクト用設定")]
    [SerializeField] private GameObject jumpDustPrefab;
    [SerializeField] private GameObject landingDustPrefab;
    [SerializeField] private GameObject runDustPrefab;
    [SerializeField] private GameObject dashDustPrefab;
    [SerializeField] private GameObject wallSlideDustPrefab;
    [SerializeField] private GameObject wallJumpDustPrefab;
    [SerializeField] private Transform dustSpawnPoint;
    [SerializeField] private Transform dashDustPoint;
    [SerializeField] private Transform wallDustLeftPoint;
    [SerializeField] private Transform wallDustRightPoint;
    [SerializeField] private Transform wallJumpDustLeftPoint;
    [SerializeField] private Transform wallJumpDustRightPoint;

    [SerializeField] private float runDustInterval = 0.12f;
    private float runDustTimer;

    private GameObject currentDashDust;
    private GameObject currentWallSlideDust;

    // SE用変数
    [Header("SE")]
    [SerializeField] private AudioClip jumpSE;
    [SerializeField, Range(0f, 1f)] private float jumpSEVolume = 1f;
    [SerializeField] private AudioClip dashSE;
    [SerializeField, Range(0f, 1f)] private float dashSEVolume = 1f;
    [SerializeField] private AudioClip deathSE;
    [SerializeField, Range(0f, 1f)] private float deathSEVolume = 1f;
    [SerializeField] private AudioClip landingSE;
    [SerializeField, Range(0f, 1f)] private float landingSEVolume = 1f;
    [SerializeField] private float landingSECooldown = 0.08f;

    private float landingSECooldownTimer;

    // カメラ覗き込み用変数
    [Header("カメラ用設定")]
    [SerializeField] private CameraLookController cameraLook;
    private Vector2 lookInput;
    public bool IsOnGround => isGrounded;

    // ===== Unityイベント =====
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        playerLayer = gameObject.layer;
        oneWayGroundLayer = LayerMask.NameToLayer(oneWayLayerName);
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

        UpdateDashTimers();         // ダッシュ管理(時間・空中回数)
        UpdateArrowTimers();        // Arrowギミック関連タイマー更新

        UpdateState();            // Player状態更新

        // アニメ・向き更新
        UpdateFacingLock();
        UpdateFacing();
        UpdateAnimator();

        // エフェクト更新
        HandleRunDust();
        HandleDashDust();
        HandleWallSlideDust();

        // SE管理
        UpdateLandingSECooldown();
    }

    private void FixedUpdate()
    {
        if (!canControl) return;

        // 圧死フラグ初期化
        pressFromLeft_Rock = pressFromRight_Rock = pressFromUp_Rock = pressFromDown_Rock = false;
        pressFromLeft_Other = pressFromRight_Other = pressFromUp_Other = pressFromDown_Other = false;


        // 横移動処理（壁ジャンプ中は操作ロック）
        if (!isDashing)
        {
            if (arrowControlLockTimer > 0f)
            {
                arrowControlLockTimer -= Time.fixedDeltaTime;
            }
            else if (wallJumpControlLockTimer > 0f)
            {
                wallJumpControlLockTimer -= Time.fixedDeltaTime;
            }
            else
            {
                rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
            }
        }

        // ダッシュ処理
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
        }

        // 壁張り付き落下速度制限
        if (currentState == PlayerState.WallSlide && !isDashing)
        {
            ApplyWallSlide();
        }

        // ジャンプ・重力
        if (!isDashing)
        {
            TryJump();              // ジャンプ入力時処理
            ApplyJumpCut();         // 可変ジャンプ調整
            ApplyDynamicGravity();  // 動的重力調整
            ApplyGroundStick();    // 地面への吸着
        }

        // 移動床の移動量を加算
        if (isOnDeltaPlatform && currentPlatform != null)
        {
            Vector2 platformVelocity = currentPlatform.Delta / Time.fixedDeltaTime;
            rb.linearVelocity += new Vector2(platformVelocity.x, 0f);
        }

        // StickyWall追従
        if (!isGrounded &&
            isOnStickyWall &&
            currentStickyWallPlatform != null)
        {
            Vector2 stickyWallVelocity = currentStickyWallPlatform.Delta / Time.fixedDeltaTime;
            Vector2 v = rb.linearVelocity;

            bool pressingAway = IsPressingAwayFromStickyWall();

            // 壁から離れる入力中でなければ、横追従を許可
            if (!pressingAway && Mathf.Abs(stickyWallVelocity.x) > 0.01f)
            {
                v.x = stickyWallVelocity.x;
            }

            // 上方向追従は、壁張り付き中だけに限定
            if (!pressingAway &&
                currentState == PlayerState.WallSlide &&
                stickyWallVelocity.y > 0.01f)
            {
                v.y = stickyWallVelocity.y;
            }

            rb.linearVelocity = v;
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
            if (collision.TryGetComponent<ItemManager>(out var item))
            {
                gameManager.CollectItem(item);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var platform = collision.collider.GetComponentInParent<IPlatformDelta>();

        if (platform != null)
        {
            currentPlatform = platform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var platform = collision.collider.GetComponentInParent<IPlatformDelta>();

        if (platform != null && currentPlatform == platform)
        {
            currentPlatform = null;
            isOnDeltaPlatform = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!canControl) return;
        if (currentState == PlayerState.Dead) return;

        bool isGW = IsInLayerMask(collision.gameObject, groundLayer)
                || IsInLayerMask(collision.gameObject, wallLayer)
                || IsInLayerMask(collision.gameObject, stickyWallLayer)
                || collision.gameObject.GetComponentInParent<RockHeadMarker>() != null;
        if (!isGW) return;

        int count = collision.contactCount;
        for (int i = 0; i < count; i++)
        {
            var cp = collision.GetContact(i);

            Collider2D other = cp.collider;     // 相手
            Collider2D self = cp.otherCollider; // 自分
            if (other == null) continue;

            bool otherIsRock = IsRockHead(other);

            // RockHead以外のGround/Wallを"Other"として扱う
            bool otherIsGW = IsInLayerMask(other.gameObject, groundLayer)
                        || IsInLayerMask(other.gameObject, wallLayer)
                        || IsInLayerMask(other.gameObject, stickyWallLayer)
                        || otherIsRock;
            bool otherIsOther = otherIsGW && !otherIsRock;

            Vector2 n = cp.normal; // 相手→Playerの法線

            // 左から押される（→方向）: normal.x が +
            if (n.x > crushNormalThreshold)
            {
                if (otherIsRock) pressFromLeft_Rock = true;
                else if (otherIsOther) pressFromLeft_Other = true;
            }
            // 右から押される（←方向）: normal.x が -
            else if (n.x < -crushNormalThreshold)
            {
                if (otherIsRock) pressFromRight_Rock = true;
                else if (otherIsOther) pressFromRight_Other = true;
            }

            // 下から押される（↑方向）: normal.y が +
            if (n.y > crushNormalThreshold)
            {
                if (otherIsRock) pressFromDown_Rock = true;
                else if (otherIsOther) pressFromDown_Other = true;
            }
            // 上から押される（↓方向）: normal.y が -
            else if (n.y < -crushNormalThreshold)
            {
                if (otherIsRock) pressFromUp_Rock = true;
                else if (otherIsOther) pressFromUp_Other = true;
            }
        }

        // ここまでで「この物理ステップ中に、どっち方向から押されたか」が溜まる
        bool stickyCrushHorizontal =
            isOnStickyWall &&
            ((pressFromLeft_Rock && isOnRightStickyWall) ||
            (pressFromRight_Rock && isOnLeftStickyWall));

        bool crushHorizontal =
            stickyCrushHorizontal ||
            (pressFromLeft_Rock && pressFromRight_Other) ||
            (pressFromRight_Rock && pressFromLeft_Other);

        bool crushVertical =
            (pressFromUp_Rock && pressFromDown_Other) ||
            (pressFromDown_Rock && pressFromUp_Other);

        if (crushHorizontal || crushVertical)
        {
            RequestDeath();
        }

        var platform = collision.collider.GetComponentInParent<IPlatformDelta>();
        if (platform != null)
        {
            // 接触点の法線が上向き（床）なら「乗ってる」
            for (int i = 0; i < collision.contactCount; i++)
            {
                var cp = collision.GetContact(i);
                if (cp.normal.y > 0.5f)
                {
                    currentPlatform = platform;
                    isOnDeltaPlatform = true;
                    break;
                }
            }
        }
    }

    // ===== InputSystemイベント部分 =====

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canControl) return;

        move = context.ReadValue<float>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        if (!canJump) return;   // Jump能力開放判定

        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (context.canceled)
        {
            jumpReleased = true;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        if (!context.performed) return;
        if (!canDash) return;
        if (dashCooldownTimer > 0f) return;

        if (!isGrounded)
        {
            if (!allowAirDash) return;
            if (airDashUsed) return;
            airDashUsed = true;
        }

        StartDash();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!canControl) return;

        lookInput = context.ReadValue<Vector2>();

        // カメラ側へ入力値の受渡
        if (cameraLook != null)
        {
            cameraLook.SetLookInput(lookInput);
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        if (!context.performed) return;
        if (!isGrounded) return;
        if (!IsOneWay()) return;

        DropThroughOneWay();
    }

    public void OnEnterPortal(InputAction.CallbackContext context)
    {
        if (!canControl) return;
        if (!context.performed) return;
        if (!IsGrounded()) return;

        if (currentGate != null)
        {
            currentGate.TryUnlock();
            return;
        }

        if (currentPortal != null)
        {
            DisablePlayerControl();
            TransitionManager.Instance.LoadScene(currentPortal.SceneName);
        }
    }

    // ===== 自作メソッド =====

    // ---能力反映メソッド群---

    // 能力反映
    public void ApplyAbilities(PlayerAbilityData data)
    {
        if (data == null) return;

        canJump = data.canJump;
        canDash = data.canDash;
        canWallSlide = data.canWallSlide;
        canWallJump = data.canWallJump;
        canDoubleJump = data.canDoubleJump;
    }

    // 能力反映メソッド呼び出し
    public void RefreshAbilities()
    {
        if (ProgressManager.Instance == null) return;

        ApplyAbilities(ProgressManager.Instance.Abilities);
    }

    // ---状態管理関連メソッド---

    // Player状態更新
    void UpdateState()
    {
        if (currentState == PlayerState.Dead)
        {
            return;
        }

        if (isDashing)
        {
            ChangeState(PlayerState.Dash);
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

        if (canWallSlide &&
            isOnWall &&
            (rb.linearVelocity.y <= 0f || (isOnStickyWall && IsStickyWallMovingUp())))
        {
            ChangeState(PlayerState.WallSlide);
            return;
        }

        if (rb.linearVelocity.y > 0.1f)
        {
            if (!isArrowBoosting && jumpCount >= maxJumpCount)
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
            isArrowBoosting = false;
            ChangeState(PlayerState.Fall);
        }
    }

    // 状態変更時処理
    void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case PlayerState.WallSlide:
                jumpCount = 0;  // 壁を地面とみなしてジャンプ回数リセット
                break;
        }
    }

    // ---動作メソッド群---

    // ジャンプ試行
    void TryJump()
    {
        if (jumpBufferCounter <= 0f) return;

        if (isGrounded || coyoteTimer > 0f)
        {
            GroundJump();
            return;
        }

        if (currentState == PlayerState.WallSlide)
        {
            WallJump();
            return;
        }

        AirJump();
    }

    // 地上ジャンプ処理
    void GroundJump()
    {
        isArrowBoosting = false;

        jumpCount = 1;
        jumpBufferCounter = 0;

        Jump();
    }

    // 空中ジャンプ処理(+2段)
    void AirJump()
    {
        if (!canDoubleJump) return;
        if (jumpCount >= maxJumpCount) return;

        isArrowBoosting = false;

        jumpCount++;
        jumpBufferCounter = 0;

        Jump();
    }

    // 壁ジャンプ処理
    void WallJump()
    {
        if (!canWallJump) return;   // 壁Jump能力解放判定

        isArrowBoosting = false;

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

        wallJumpControlLockTimer = wallJumpControlLockTime; // 操作ロックタイマーセット

        SpawnWallJumpDust();

        AudioManager.Instance.PlaySE(jumpSE, jumpSEVolume);

        // 完全停止壁からの壁ジャンプ時オールリセット
        currentStickyWallPlatform = null;
        isOnStickyWall = false;
        isOnLeftStickyWall = false;
        isOnRightStickyWall = false;
    }

    // ジャンプ処理本体
    void Jump()
    {
        groundIgnoreTimer = groundIgnoreTime;   // 地面判定無視時間セット

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        SpawnJumpDust();

        AudioManager.Instance.PlaySE(jumpSE, jumpSEVolume);
    }

    // 壁張り付き処理
    void ApplyWallSlide()
    {
        if (!isOnWall) return;
        if (isGrounded) return;
        if (rb.linearVelocity.y > 0) return;

        // 完全停止壁時
        if (isOnStickyWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            return;
        }

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

    // ダッシュ開始処理
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        dashDir = GetDashDirection();

        // ダッシュ中向き固定
        isFacingLocked = true;
        facingLockTimer = dashDuration;

        rb.linearVelocity = Vector2.zero;   // 開始時に速度リセット

        AudioManager.Instance.PlaySE(dashSE, dashSEVolume);
    }

    // ダッシュ方向取得
    float GetDashDirection()
    {
        if (move > 0.01f) return 1f;
        if (move < -0.01f) return -1f;
        return isFacingRight ? 1f : -1f;
    }

    // ---判定更新メソッド群---

    // 地面状態更新
    void UpdateGroundState()
    {
        bool rawGrounded = IsGrounded();

        // ジャンプ直後の接地無視は、上昇中のみ有効
        if (groundIgnoreTimer > 0f && rb.linearVelocity.y > 0.05f)
        {
            groundIgnoreTimer -= Time.deltaTime;
            isGrounded = false;
        }
        else
        {
            groundIgnoreTimer = 0f;
            isGrounded = rawGrounded;
        }

        if (isGrounded)
        {
            jumpCount = 0;
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer = Mathf.Max(coyoteTimer - Time.deltaTime, 0f);

            // 地面から落下時、通常ジャンプ使用済み扱い
            if (coyoteTimer <= 0f && jumpCount == 0)
            {
                //Debug.Log("Coyote Time Expired");
                jumpCount = 1;
            }
        }

        bool landedThisFrame = !wasGrounded && isGrounded;

        if (landedThisFrame)
        {
            SpawnLandingDust();
            
            if (landingSECooldownTimer <= 0f)
            {
                AudioManager.Instance.PlaySE(landingSE, landingSEVolume);
                landingSECooldownTimer = landingSECooldown;
            }
        }

        wasGrounded = isGrounded;
    }

    // 地面判定
    bool IsGrounded()
    {
        LayerMask groundMask;

        if (rb.linearVelocity.y > 0.05f)
        {
            groundMask = groundLayer;     // 上昇中:OneWay無視
        }
        else
        {
            groundMask = groundLayer | oneWayLayer;
        }

        Vector3 leftStartPoint = transform.position - Vector3.right * 0.3f + Vector3.up * 0.05f;
        Vector3 rightStartPoint = transform.position + Vector3.right * 0.3f + Vector3.up * 0.05f;
        Vector3 endPoint = transform.position - Vector3.up * 0.1f;

        Debug.DrawLine(leftStartPoint, endPoint, Color.yellow);
        Debug.DrawLine(rightStartPoint, endPoint,Color.yellow);

        return Physics2D.Linecast(leftStartPoint, endPoint, groundMask)
            || Physics2D.Linecast(rightStartPoint, endPoint, groundMask);
    }

    // OnyWay床判定
    bool IsOneWay()
    {
        Vector3 leftStartPoint = transform.position - Vector3.right * 0.3f;
        Vector3 rightStartPoint = transform.position + Vector3.right * 0.3f;
        Vector3 endPoint = transform.position - Vector3.up * 0.1f;

        return Physics2D.Linecast(leftStartPoint, endPoint, oneWayLayer)
            || Physics2D.Linecast(rightStartPoint, endPoint, oneWayLayer);
    }

    // 壁状態更新
    void UpdateWallState()
    {
        if (isGrounded)
        {
            isOnWall = false;
            isOnLeftWall = false;
            isOnRightWall = false;

            isOnStickyWall = false;
            isOnLeftStickyWall = false;
            isOnRightStickyWall = false;

            currentStickyWallPlatform = null;
            stickyWallGraceTimer = 0f;

            return;
        }

        bool leftNormal = CheckLeftWall();
        bool rightNormal = CheckRightWall();

        IPlatformDelta leftStickyPlatform;
        IPlatformDelta rightStickyPlatform;

        bool leftStickyNow = CheckLeftStickyWall(out leftStickyPlatform);
        bool rightStickyNow = CheckRightStickyWall(out rightStickyPlatform);

        if (leftStickyNow || rightStickyNow)
        {
            stickyWallGraceTimer = stickyWallGraceTime;

            isOnLeftStickyWall = leftStickyNow;
            isOnRightStickyWall = rightStickyNow;
            isOnStickyWall = true;

            currentStickyWallPlatform = leftStickyPlatform ?? rightStickyPlatform;
        }
        else
        {
            stickyWallGraceTimer -= Time.deltaTime;

            if (stickyWallGraceTimer <= 0f)
            {
                isOnLeftStickyWall = false;
                isOnRightStickyWall = false;
                isOnStickyWall = false;
                currentStickyWallPlatform = null;
            }
        }

        isOnLeftWall = leftNormal || isOnLeftStickyWall;
        isOnRightWall = rightNormal || isOnRightStickyWall;
        isOnWall = isOnLeftWall || isOnRightWall;

        if (!isOnStickyWall)
        {
            currentStickyWallPlatform = null;
        }
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

    // 左壁判定(完全停止壁)
    bool CheckLeftStickyWall(out IPlatformDelta platform)
    {
        platform = null;

        Vector3 upper = transform.position + Vector3.up * 1.0f;
        Vector3 lower = transform.position + Vector3.up * 0.4f;

        Vector3 upperEnd = upper - Vector3.right * 0.6f;
        Vector3 lowerEnd = lower - Vector3.right * 0.6f;

        RaycastHit2D hit1 = Physics2D.Linecast(upper, upperEnd, stickyWallLayer);
        RaycastHit2D hit2 = Physics2D.Linecast(lower, lowerEnd, stickyWallLayer);

        RaycastHit2D hit = hit1.collider != null ? hit1 : hit2;

        if (hit.collider != null)
        {
            platform = hit.collider.GetComponentInParent<IPlatformDelta>();
            return true;
        }

        return false;
    }
    // 右壁判定(完全停止壁)
    bool CheckRightStickyWall(out IPlatformDelta platform)
    {
        platform = null;

        Vector3 upper = transform.position + Vector3.up * 1.0f;
        Vector3 lower = transform.position + Vector3.up * 0.4f;

        Vector3 upperEnd = upper + Vector3.right * 0.6f;
        Vector3 lowerEnd = lower + Vector3.right * 0.6f;

        RaycastHit2D hit1 = Physics2D.Linecast(upper, upperEnd, stickyWallLayer);
        RaycastHit2D hit2 = Physics2D.Linecast(lower, lowerEnd, stickyWallLayer);

        RaycastHit2D hit = hit1.collider != null ? hit1 : hit2;

        if (hit.collider != null)
        {
            platform = hit.collider.GetComponentInParent<IPlatformDelta>();
            return true;
        }

        return false;
    }

    // ダッシュ管理
    void UpdateDashTimers()
    {
        // クールタイム
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // ダッシュ中の残り時間
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer < 0f)
            {
                isDashing = false;
            }
        }

        if (isGrounded || currentState == PlayerState.WallSlide)
        {
            airDashUsed = false;
        }
    }

    void UpdateArrowTimers()
    {
        if (arrowJumpCutIgnoreTimer > 0f)
        {
            arrowJumpCutIgnoreTimer -= Time.deltaTime;
        }
    }

    // StickyWall上昇時判定
    bool IsStickyWallMovingUp()
    {
        if (currentStickyWallPlatform == null) return false;

        Vector2 stickyWallVelocity = currentStickyWallPlatform.Delta / Time.fixedDeltaTime;
        return stickyWallVelocity.y > 0.01f;
    }

    // ---ユーティリティメソッド群---

    // LayerMask内にオブジェクトが含まれるかの判定
    static bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }

    // RockHeadかの判定
    static bool IsRockHead(Collider2D col)
    {
        return col != null && col.GetComponentInParent<RockHeadMarker>() != null;
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

        if (isOnStickyWall && currentState == PlayerState.WallSlide) return;

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

    // 壁との逆方向入力判定
    bool IsPressingAwayFromStickyWall()
    {
        if (isOnLeftStickyWall && move > 0.01f) return true;
        if (isOnRightStickyWall && move < -0.01f) return true;
        return false;
    }

    // ---エフェクト関連メソッド群---

    // ジャンプエフェクト生成
    void SpawnJumpDust()
    {
        if (jumpDustPrefab == null || dustSpawnPoint == null) return;

        Instantiate(jumpDustPrefab, dustSpawnPoint.position, Quaternion.identity);
    }

    // 着地エフェクト生成
    void SpawnLandingDust()
    {
        if (landingDustPrefab == null || dustSpawnPoint == null) return;

        Instantiate(landingDustPrefab, dustSpawnPoint.position, Quaternion.identity);
    }

    // 走行エフェクト生成
    void SpawnRunDust()
    {
        if (runDustPrefab == null || dustSpawnPoint == null) return;

        Instantiate(runDustPrefab, dustSpawnPoint.position, Quaternion.identity);
    }

    // 壁ジャンプエフェクト生成
    void SpawnWallJumpDust()
    {
        if (wallJumpDustPrefab == null) return;

        Transform spawnPoint = isOnLeftWall ? wallJumpDustLeftPoint : wallJumpDustRightPoint;

        if (spawnPoint == null) return;

        Instantiate(wallJumpDustPrefab, spawnPoint.position, Quaternion.identity);
    }

    // 走行エフェクト管理
    void HandleRunDust()
    {
        bool shouldSpawnRunDust =
            isGrounded &&
            Mathf.Abs(move) > 0.1f &&
            currentState == PlayerState.Run &&
            !isDashing;

        if (!shouldSpawnRunDust)
        {
            runDustTimer = 0f;
            return;
        }

        runDustTimer -= Time.deltaTime;

        if (runDustTimer <= 0f)
        {
            SpawnRunDust();
            runDustTimer = runDustInterval;
        }
    }

    // 着地SEクールタイム管理
    void UpdateLandingSECooldown()
    {
        if (landingSECooldownTimer > 0f)
        {
            landingSECooldownTimer -= Time.deltaTime;
        }
    }

    // ダッシュエフェクト管理
    void HandleDashDust()
    {
        if (isDashing)
        {
            if (currentDashDust == null)
            {
                Quaternion rot = dashDir > 0
                    ? Quaternion.identity
                    : Quaternion.Euler(0f, 180f, 0f);

                currentDashDust = Instantiate(
                    dashDustPrefab,
                    dashDustPoint.position,
                    rot,
                    transform
                );
            }
        }
        else
        {
            if (currentDashDust != null)
            {
                Destroy(currentDashDust);
                currentDashDust = null;
            }
        }
    }

    // 壁滑りエフェクト生成及び管理
    void HandleWallSlideDust()
    {
        bool shouldWallSlideDust =
        currentState == PlayerState.WallSlide &&
        isOnWall &&
        !isGrounded &&
        !isDashing;

        if (shouldWallSlideDust)
        {
            Transform spawnPoint = isOnLeftWall ? wallDustLeftPoint : wallDustRightPoint;
            if (wallSlideDustPrefab == null || spawnPoint == null) return;

            if (currentWallSlideDust == null)
            {
                currentWallSlideDust = Instantiate(
                    wallSlideDustPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    transform
                );
            }

            currentWallSlideDust.transform.position = spawnPoint.position;
        }
        else
        {
            if (currentWallSlideDust != null)
            {
                var ps = currentWallSlideDust.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }

                Destroy(currentWallSlideDust, 0.3f);
                currentWallSlideDust = null;
            }
        }
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

        if (currentState == PlayerState.WallSlide && isOnStickyWall)
        {
            rb.gravityScale = 0f;
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

    // 可変ジャンプ調整
    void ApplyJumpCut()
    {
        if (!jumpReleased) return;

        if (arrowJumpCutIgnoreTimer > 0f)
        {
            jumpReleased = false;
            return;
        }

        jumpReleased = false;

        if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    // 横ずれ防止処理
    void ApplyGroundStick()
    {
        if (!isGrounded) return;
        if (isDashing) return;
        if (Mathf.Abs(move) > 0.01f) return;
        if (rb.linearVelocity.y > 0.05f) return;
        if (currentState == PlayerState.WallSlide) return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // すり抜け処理
    void DropThroughOneWay()
    {
        CancelInvoke(nameof(EnableOneWayCollision));    // 多重発火対策

        groundIgnoreTimer = Mathf.Max(groundIgnoreTimer, dropThroughTime);  // 地面判定無視タイマー設定

        // PlyerとOneWayGround間の衝突を一時的に解除
        Physics2D.IgnoreLayerCollision(playerLayer, oneWayGroundLayer, true);

        Invoke(nameof(EnableOneWayCollision), dropThroughTime); // 時間経過後戻す
    }

    // 衝突解除
    void EnableOneWayCollision()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, oneWayGroundLayer, false);
    }

    // バウンド処理
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

        AudioManager.Instance.PlaySE(deathSE, deathSEVolume);

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

        ClearInputAndTransientStates();

        rb.linearVelocity = Vector2.zero;   //　速度停止
        rb.angularVelocity = 0f;            //　回転停止
        rb.simulated = false;               //　物理停止
    }

    // 入力・一時状態リセット
    void ClearInputAndTransientStates()
    {
        // 入力系
        lookInput = Vector2.zero;
        jumpReleased = false;

        // ジャンプバッファ
        jumpBufferCounter = 0f;
        coyoteTimer = 0f;

        // ダッシュ系
        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
        airDashUsed = false;

        // 壁ジャンプロック類
        wallJumpControlLockTimer = 0f;
        facingLockTimer = 0f;
        isFacingLocked = false;

        // 地面無視タイマー（落下直後の変な判定防止）
        groundIgnoreTimer = 0f;
    }

    // Appearアニメイベント(操作可能切り替え)
    public void OnAppearFinished()
    {
        rb.simulated = true;

        isGrounded = false;
        wasGrounded = false;
        isOnWall = false;
        isOnLeftWall = false;
        isOnRightWall = false;

        canControl = true;
        ChangeState(PlayerState.Idle);
    }

    // リスポーン処理
    public void RespawnTo(Vector3 pos, bool playAppear = true)
    {
        DisablePlayerControl();

        transform.position = pos;   // 保存位置に移動

        // 状態・アニメの強制リセット
        ChangeState(PlayerState.Idle);
        animator.SetInteger("State", (int)PlayerState.Idle);

        if (playAppear)
        {
            // Appearアニメ再生
            animator.ResetTrigger("Appear");
            animator.SetTrigger("Appear");
        }
    }

    // Hubポータル入口用メソッド
    public void SetCurrentPortal(PortalEntrance portal)
    {
        currentPortal = portal;
    }

    public void ClearCurrentPortal(PortalEntrance portal)
    {
        if (currentPortal == portal) currentPortal = null;
    }

    // 解放ゲート用メソッド
    public void SetCurrentGate(GateTrigger gate)
    {
        currentGate = gate;
    }

    public void ClearCurrentGate(GateTrigger gate)
    {
        if (currentGate == gate)
        {
            currentGate = null;
        }
    }

    // Arrowギミック用メソッド
    public void ApplyArrowBoost(Vector2 velocity)
    {
        if (!canControl) return;
        if (currentState == PlayerState.Dead) return;

        isArrowBoosting = true;

        // ダッシュ中なら解除
        isDashing = false;
        dashTimer = 0f;

        wallJumpControlLockTimer = 0f;
        jumpBufferCounter = 0;
        jumpReleased = false;

        rb.linearVelocity = velocity;

        arrowControlLockTimer = arrowControlLockTime; // 操作ロックタイマーセット
        arrowJumpCutIgnoreTimer = arrowJumpCutIgnoreTime; // ジャンプカット無視タイマーセット
        groundIgnoreTimer = groundIgnoreTime;

        jumpCount = 1;       // 2段ジャンプ使用済み時、ジャンプ回数を1にリセット
        airDashUsed = false; // 空中ダッシュ回数リセット
    }
}
