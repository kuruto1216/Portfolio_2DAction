using UnityEngine;

public class DustAutoDestroy : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 1f;

    private void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}
