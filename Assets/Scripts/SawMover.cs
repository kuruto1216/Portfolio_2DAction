using UnityEngine;
using DG.Tweening;

public class SawMover : MonoBehaviour
{
    public float moveDistance = 3f; // ˆÚ“®‹——£
    public float duration = 2f; // ˆÚ“®‚É‚©‚©‚éŽžŠÔ
    Tween moveTween;

    private void Start()
    {
        moveTween = transform.DOMoveX(transform.position.x + moveDistance, duration)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        moveTween.Kill();
    }
}
