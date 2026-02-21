using UnityEngine;

public class JumpPadTrigger : MonoBehaviour
{
    [SerializeField] private JumpPad jumpPad;

    private void Reset()
    {
        jumpPad = GetComponentInParent<JumpPad>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerManager>();
        if (player == null) return;

        jumpPad.OnStepped(player);
    }
}
