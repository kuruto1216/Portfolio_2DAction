using UnityEngine;

public class AlternatingBurnerAnimRelay : MonoBehaviour
{
    [SerializeField] private AlternatingBurnerController burner;

    private void Reset()
    {
        burner = GetComponentInParent<AlternatingBurnerController>();
    }

    public void EnableHit()
    {
        burner.EnableHit();
    }

    public void DisableHit()
    {
        burner.DisableHit();
    }
}
