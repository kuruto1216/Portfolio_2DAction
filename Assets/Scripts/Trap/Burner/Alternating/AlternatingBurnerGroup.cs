using UnityEngine;
using System.Collections;

public class AlternatingBurnerGroup : MonoBehaviour
{
    [Header("Group A")]
    [SerializeField] private AlternatingBurnerController[] groupA;

    [Header("Group B")]
    [SerializeField] private AlternatingBurnerController[] groupB;

    [Header("ŠJŽnÝ’è")]
    [SerializeField] private float startDelay = 0f;

    [Header("Ø‚è‘Ö‚¦ŠÔŠu")]
    [SerializeField] private float intervalAfterOff = 0.2f;

    private int currentIndex;
    private bool isRunning;
    private bool isGroupMode;
    private bool isAActive = true;
    private int waitingFinishCount;
    private Coroutine switchCo;

    private void OnEnable()
    {
        isRunning = true;
        currentIndex = 0;
        isAActive = true;

        isGroupMode = groupA.Length > 0 && groupB.Length > 0;

        RegisterGroup(groupA);
        RegisterGroup(groupB);

        ForceOffGroup(groupA);
        ForceOffGroup(groupB);

        StartCoroutine(StartRoutine());
    }

    private void OnDisable()
    {
        isRunning = false;

        if (switchCo != null) StopCoroutine(switchCo);

        UnregisterGroup(groupA);
        UnregisterGroup(groupB);
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (!isRunning) yield break;

        if (isGroupMode)
        {
            StartGroup(groupA);
            isAActive = true;
        }
        else
        {
            if (groupA.Length == 0) yield break;

            currentIndex = 0;
            groupA[currentIndex].StartBurner();
        }
    }

    private void HandleBurnerFinished(AlternatingBurnerController finishedBurner)
    {
        if (!isRunning) return;

        if (isGroupMode)
        {
            waitingFinishCount--;

            if (waitingFinishCount > 0) return;

            if (switchCo != null) StopCoroutine(switchCo);

            switchCo = StartCoroutine(SwitchGroupRoutine());
        }
        else
        {
            if (switchCo != null) StopCoroutine(switchCo);

            switchCo = StartCoroutine(SwitchSingleRoutine());
        }
    }

    private IEnumerator SwitchSingleRoutine()
    {
        yield return new WaitForSeconds(intervalAfterOff);

        if (!isRunning) yield break;
        if (groupA.Length == 0) yield break;

        currentIndex++;

        if (currentIndex >= groupA.Length) currentIndex = 0;

        groupA[currentIndex].StartBurner();
    }

    private IEnumerator SwitchGroupRoutine()
    {
        yield return new WaitForSeconds(intervalAfterOff);

        if (!isRunning) yield break;

        if (isAActive)
        {
            StartGroup(groupB);
            isAActive = false;
        }
        else
        {
            StartGroup(groupA);
            isAActive = true;
        }
    }

    private void StartGroup(AlternatingBurnerController[] group)
    {
        waitingFinishCount = 0;

        foreach (var burner in group)
        {
            if (burner == null) continue;

            waitingFinishCount++;
            burner.StartBurner();
        }
    }

    private void ForceOffGroup(AlternatingBurnerController[] group)
    {
        foreach (var burner in group)
        {
            if (burner != null)
                burner.ForceOff();
        }
    }

    private void RegisterGroup(AlternatingBurnerController[] group)
    {
        foreach (var burner in group)
        {
            if (burner != null)
                burner.OnBurnerFinished += HandleBurnerFinished;
        }
    }

    private void UnregisterGroup(AlternatingBurnerController[] group)
    {
        foreach (var burner in group)
        {
            if (burner != null)
                burner.OnBurnerFinished -= HandleBurnerFinished;
        }
    }
}
