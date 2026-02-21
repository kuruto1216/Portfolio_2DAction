using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("上方向へのバウンス力")]
    [SerializeField] private float bouncePower = 12f;

    [Header("バウンスのクールダウン時間")]
    [SerializeField] private float cooldown = 0.5f;

    [SerializeField] private Animator visualAnimator;

    private float _lastTime = -999f;
    private PlayerManager _pendingPlayer;

    public void OnStepped(PlayerManager player)
    {
        if (Time.time < _lastTime + cooldown) return;

        _pendingPlayer = player;
        visualAnimator.SetTrigger("Press");
        _lastTime = Time.time;
    }

    public void DoBounce()
    {
        if (_pendingPlayer == null) return;

        _pendingPlayer.Bounce(bouncePower);
        _pendingPlayer = null;
    }
}
