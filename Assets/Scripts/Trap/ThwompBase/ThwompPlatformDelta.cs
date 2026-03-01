using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ThwompPlatformDelta : MonoBehaviour, IPlatformDelta
{
    public Vector2 Delta { get; private set; }

    private Rigidbody2D rb;
    private Vector2 prevPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        prevPos = rb.position;
    }

    private void FixedUpdate()
    {
        Vector2 current = rb.position;
        Delta = current - prevPos;
        prevPos = current;
    }
}
