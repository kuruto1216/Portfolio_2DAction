using System.Runtime.CompilerServices;
using UnityEngine;

public class CircleMover : MonoBehaviour
{
    [Header("Circle Move Settings")]
    [SerializeField] private Transform center;
    [SerializeField] private float radius = 2f;
    [SerializeField] private float angularSpeed = 90f;
    [SerializeField] private bool clockwise = true;
    [SerializeField] private float startAngle = 0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animatorSpeed = 1f;

    private float currentAngle;

    private void Start()
    {
        if (animator != null)
        {
            animator.speed = animatorSpeed;
        }

        currentAngle = startAngle;

        ApplyCirclePosition();
    }

    private void Update()
    {
        if (center == null) return;

        float direction = clockwise ? -1f : 1f;

        currentAngle += angularSpeed * direction * Time.deltaTime;

        ApplyCirclePosition();
    }

    private void ApplyCirclePosition()
    {
        if (center == null) return;

        float rad = currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;

        transform.position = center.position + offset;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (center == null) return;

        Gizmos.color = Color.cyan;

        const int segments = 64;
        Vector3 prev = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = 360f / segments * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;

            if (i > 0)
            {
                Gizmos.DrawLine(prev,pos);
            }

            prev = pos;
        }
    }
#endif
}
