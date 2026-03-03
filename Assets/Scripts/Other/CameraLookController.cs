using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraLookController : MonoBehaviour
{
    // ===== 変数 =====

    [Header("References")]
    [SerializeField] CinemachineCamera cmCamera;
    [SerializeField] PlayerManager player;

    [Header("Look Settings")]
    [SerializeField] Vector2 lookOffset = new Vector2(2.0f, 2.0f);  //(左右, 上下)
    [SerializeField] float smooth = 10f;            // 大きいほど速く追従
    [SerializeField] float holdToActivate = 0.15f;  // 長押し時間
    [SerializeField] float inputThreshold = 0.5f;
    [SerializeField] float lookUpMultiplier = 0.7f;     // 上方向は弱め
    [SerializeField] float airLookMultiplier = 0.6f;    // 空中は弱め

    CinemachinePositionComposer composer;

    Vector2 lookInput;
    float holdTimer;

    Vector2 targetOffset;
    Vector2 currentOffset;

    // ===== Unityイベント =====

    private void Awake()
    {
        composer = cmCamera.GetComponent<CinemachinePositionComposer>();
    }

    private void Update()
    {
        bool grounded = player != null && player.IsOnGround;

        bool lookingX = Mathf.Abs(lookInput.x) > inputThreshold;
        bool lookingY = Mathf.Abs(lookInput.y) > inputThreshold;

        if (lookingX || lookingY) holdTimer += Time.deltaTime;
        else holdTimer = 0f;

        if (holdTimer >= holdToActivate)
        {
            float tx = lookingX ? Mathf.Sign(lookInput.x) * lookOffset.x : 0f;
            float ty = 0f;

            if (lookingY)
            {
                float signY = Mathf.Sign(lookInput.y);

                // 上方向だけ弱める
                if (signY > 0)
                {
                    ty = signY * lookOffset.y * lookUpMultiplier;
                }
                else
                {
                    ty = signY * lookOffset.y;
                }
            }

            targetOffset = new Vector2(tx, ty);
        }
        else
        {
            targetOffset = Vector2.zero;
        }

        if (!grounded)
        {
            targetOffset = new Vector2(targetOffset.x, targetOffset.y * airLookMultiplier);
        }

        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        currentOffset = Vector2.Lerp(currentOffset, targetOffset, t);

        var offset = composer.TargetOffset;
        offset.x = currentOffset.x;
        offset.y = currentOffset.y;
        composer.TargetOffset = offset;
    }

    // PlayerManagerからの入力受け取り
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }
}
