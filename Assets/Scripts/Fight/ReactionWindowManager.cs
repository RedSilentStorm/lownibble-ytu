using System;
using System.Collections;
using UnityEngine;

public class ReactionWindowManager : MonoBehaviour
{
    public static ReactionWindowManager Instance { get; private set; }

    [SerializeField] private float windowDuration = 0.8f;
    private bool windowOpen = false;
    private Action onParry, onDodge, onWindowExpired;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenReactionWindow(Action parryCallback, Action dodgeCallback, Action expiredCallback)
    {
        if (windowOpen) return;
        windowOpen = true;
        onParry = parryCallback;
        onDodge = dodgeCallback;
        onWindowExpired = expiredCallback;

        StartCoroutine(WindowCoroutine());
    }

    private IEnumerator WindowCoroutine()
    {
        // Görsel ipucu göster
        ReactionUI.Instance?.ShowWindow();
        yield return new WaitForSeconds(windowDuration);
        ReactionUI.Instance?.HideWindow();
        // Süre doldu, callback
        onWindowExpired?.Invoke();
        windowOpen = false;
    }

    public void ParryPressed()
    {
        if (!windowOpen) return;
        StopAllCoroutines();
        windowOpen = false;
        ReactionUI.Instance?.HideWindow();
        onParry?.Invoke();
    }

    public void DodgePressed()
    {
        if (!windowOpen) return;
        StopAllCoroutines();
        windowOpen = false;
        ReactionUI.Instance?.HideWindow();
        onDodge?.Invoke();
    }
}