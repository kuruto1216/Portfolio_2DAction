using UnityEngine;
using System;
using System.Collections;

public class AlternatingBurnerController : MonoBehaviour
{
    public event Action<AlternatingBurnerController> OnBurnerFinished;

    [Header("時間設定")]
    [SerializeField] private float onTime = 1.5f;

    [Header("参照")]
    [SerializeField] private Collider2D hitTrigger;
    [SerializeField] private Animator visualAnimator;

    [Header("Animator Parameter")]
    [SerializeField] private string animParamIsOn = "IsOn";

    private Coroutine _timerCo;
    private bool isActive;

    private void Reset()
    {
        hitTrigger = GetComponentInChildren<Collider2D>();
        visualAnimator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        ForceOff();
    }

    private void OnDisable()
    {
        isActive = false;

        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = null;
    }

    public void StartBurner()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);

        _timerCo = null;
        isActive = true;

        SetOn(true);
    }

    public void ForceOff()
    {
        isActive = false;

        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = null;

        if (hitTrigger != null) hitTrigger.enabled = false;

        SetOn(false);
    }

    private void SetOn(bool value)
    {
        if (visualAnimator != null) visualAnimator.SetBool(animParamIsOn, value);
    }

    // アニメイベント：Switchステート終了時
    public void EnableHit()
    {
        if (!isActive) return;

        if (hitTrigger != null) hitTrigger.enabled = true;

        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = StartCoroutine(OnDuration());
    }

    private IEnumerator OnDuration()
    {
        yield return new WaitForSeconds(onTime);

        SetOn(false);
    }

    // アニメイベント：Offステート開始時
    public void DisableHit()
    {
        if (hitTrigger != null) hitTrigger.enabled = false;

        if (_timerCo != null) StopCoroutine(_timerCo);
        _timerCo = null;

        if (!isActive) return;

        isActive = false;

        OnBurnerFinished?.Invoke(this);
    }
}
