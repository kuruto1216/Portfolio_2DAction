using UnityEngine;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class BurnerController : MonoBehaviour
{
    [Header("時間設定（秒）")]
    [SerializeField] private float onTime = 1.5f; // 点灯時間
    [SerializeField] private float offTime = 1.5f; // 消灯時間
    [SerializeField] private float startDelay = 0.0f; // 開始前の遅延時間

    [Header("参照")]
    [SerializeField] private Collider2D hitTrigger;
    [SerializeField] private Animator visualAnimator;

    [Header("Animator Parameter")]
    [SerializeField] private string animParamIsOn = "IsOn";

    private Coroutine _timerCo;

    private void Reset()
    {
        hitTrigger = GetComponentInChildren<Collider2D>();
        visualAnimator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        hitTrigger.enabled = false;
        SetOn(false);

        StartCoroutine(StartAfterDelay());  // 開始前の遅延を考慮して処理開始
    }

    private void OnDisable()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = null;
    }

    //　遅延用のコルーチン
    IEnumerator StartAfterDelay()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        SetOn(true);
    }

    //　アニメの状態切り替え
    void SetOn(bool value)
    {
        if (visualAnimator != null)
        {
            visualAnimator.SetBool(animParamIsOn, value);
        }
    }

    //　アニメイベント(Switchステート終了時)
    public void EnableHit()
    {
        hitTrigger.enabled = true;

        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = StartCoroutine(OnDuration());
    }

    //　点灯時間のコルーチン
    IEnumerator OnDuration()
    {
        yield return new WaitForSeconds(onTime);

        SetOn(false);
    }

    //　アニメイベント(Offステート開始時)
    public void DisableHit()
    {
        hitTrigger.enabled = false;

        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = StartCoroutine(OffDuration());
    }

    //　消灯時間のコルーチン
    IEnumerator OffDuration()
    {
        yield return new WaitForSeconds(offTime);

        SetOn(true);
    }
}
