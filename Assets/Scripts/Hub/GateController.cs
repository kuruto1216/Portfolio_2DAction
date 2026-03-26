using UnityEngine;
using System.Collections;

public class GateController : MonoBehaviour
{
    public enum GateAreaType
    {
        Area2,
        Area3
    }

    [SerializeField] private GateAreaType areaType;
    [SerializeField] private Transform gateVisualRoot;
    [SerializeField] private Collider2D gateCollider;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float openDuration = 0.5f;

    private bool isOpened;
    private Vector3 closedPos;
    private Vector3 openedPos;

    private void Awake()
    {
        if (gateVisualRoot == null)
        {
            gateVisualRoot = transform;
        }

        closedPos = gateVisualRoot.position;
        openedPos = closedPos + Vector3.up * openDistance;
    }

    private void Start()
    {
        if (IsAlreadyUnlocked())
        {
            ApplyOpenedImmediate();
        }
    }

    public bool CanUnlock()
    {
        if (ProgressManager.Instance == null) return false;

        return areaType switch
        {
            GateAreaType.Area2 => ProgressManager.Instance.CanUnlockArea2(),
            GateAreaType.Area3 => ProgressManager.Instance.CanUnlockArea3(),
            _ => false
        };
    }

    public bool IsUnlocked()
    {
        if (ProgressManager.Instance == null) return false;

        return areaType switch
        {
            GateAreaType.Area2 => ProgressManager.Instance.IsArea2Unlocked(),
            GateAreaType.Area3 => ProgressManager.Instance.IsArea3Unlocked(),
            _ => false
        };
    }

    public void TryUnlock()
    {
        if (isOpened) return;
        if (!CanUnlock()) return;

        switch (areaType)
        {
            case GateAreaType.Area2:
                ProgressManager.Instance.UnlockArea2();
                break;
            case GateAreaType.Area3:
                ProgressManager.Instance.UnlockArea3();
                break;
        }

        StartCoroutine(CoOpen());
    }

    private IEnumerator CoOpen()
    {
        isOpened = true;

        Vector3 startPos = gateVisualRoot.position;
        Vector3 endPos = openedPos;

        float time = 0f;

        while (time < openDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / openDuration);
            float eased = t * t * (3f - 2f * t);

            gateVisualRoot.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        gateVisualRoot.position = endPos;

        if (gateCollider != null)
        {
            gateCollider.enabled = false;
        }
    }

    private bool IsAlreadyUnlocked()
    {
        return IsUnlocked();
    }

    private void ApplyOpenedImmediate()
    {
        isOpened = true;
        gateVisualRoot.position = openedPos;

        if (gateCollider != null)
        {
            gateCollider.enabled = false;
        }
    }

    public int GetPhase()
    {
        return areaType switch
        {
            GateAreaType.Area2 => 1,
            GateAreaType.Area3 => 2,
            _ => 0
        };
    }

    public int GetRequiredFruitCount()
    {
        if (ProgressManager.Instance == null) return 0;

        return ProgressManager.Instance.GetRequiredFruitForPhase(GetPhase());
    }
}
