using UnityEngine;

public class JumpPadAnimEventRelay : MonoBehaviour
{
    [SerializeField] private JumpPad jumpPad;

    private void Reset()
    {
        jumpPad = GetComponentInParent<JumpPad>();
    }

    public void OnBounceFrame()
    {
        jumpPad.DoBounce();
    }
}
