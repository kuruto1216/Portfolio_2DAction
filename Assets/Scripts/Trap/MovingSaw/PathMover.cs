using UnityEngine;

public class PathMover : MonoBehaviour
{
    [Header("Path Points")]
    [SerializeField] private Transform[] points;

    [Header("Move Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true;

    private int currentIndex = 0;

    private void Start()
    {
        if (points == null || points.Length < 2) return;

        transform.position = points[0].position;
        currentIndex = 1;
    }

    private void Update()
    {
        if (points == null || points.Length < 2) return;

        Transform target = points[currentIndex];
        Vector3 current = transform.position;

        transform.position = Vector3.MoveTowards(current, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            transform.position = target.position;

            currentIndex++;

            if (currentIndex >= points.Length)
            {
                if (loop)
                {
                    currentIndex = 0;
                }
                else
                {
                    enabled = false;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (points == null || points.Length < 2) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < points.Length - 1; i++)
        {
            if (points[i] != null && points[i + 1] != null)
            {
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }

        if (loop && points[0] != null && points[points.Length - 1] != null)
        {
            Gizmos.DrawLine(points[points.Length - 1].position, points[0].position);
        }
    }
#endif
}
