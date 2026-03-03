using UnityEngine;

public class PortalEntrance : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] string sceneName;

    public string SceneName => sceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm != null) pm.SetCurrentPortal(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerManager>();
        if (pm != null) pm.ClearCurrentPortal(this);
    }
}
