using UnityEngine;

public class BurnerAnimRelay : MonoBehaviour
{
    [SerializeField] private BurnerController burner;

    private void Reset()
    {
        burner = GetComponentInParent<BurnerController>();
    }

    //　アニメイベント(Switchステート終了時)
    public void EnableHit()
    {
        burner.EnableHit();
    }

    //　アニメイベント(Offステート開始時)
    public void DisableHit()
    {
        burner.DisableHit();
    }
}
