using UnityEngine;

public class PlayerVisualEvents : MonoBehaviour
{
    PlayerManager player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerManager>();
    }

    public void OnAppearFinished()
    {
        player.OnAppearFinished();
    }

    public void Death()
    {
        player.Death();
    }
}
