using System.Collections;
using UnityEngine;

public class Fan : MonoBehaviour
{
    public float maxPower = 8f; // 扇風機の最大風力
    public float changeSpeed = 2f; // 風力の変化速度
    public float onDuration = 2f; // 扇風機がONになる時間
    public float offDuration = 2f; // 扇風機がOFFになる時間

    float currentPower = 0f;
    float targetPower = 0f;

    bool isOn = false;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(FanLoop());
    }

    IEnumerator FanLoop()
    {
        while (true)
        {
            isOn = true;
            targetPower = maxPower;
            yield return new WaitForSeconds(onDuration);

            isOn = false;
            targetPower = 0f;
            yield return new WaitForSeconds(offDuration);
        }
    }

    private void Update()
    {
        currentPower = Mathf.Lerp(currentPower, targetPower, Time.deltaTime * changeSpeed);

        animator.speed = currentPower / maxPower; // アニメーション速度を風力に応じて変化させる
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            rb.AddForce(Vector2.up * currentPower);
        }
        
    }
}
