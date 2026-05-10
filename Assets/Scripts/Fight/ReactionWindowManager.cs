using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionWindowManager : MonoBehaviour
{
    public static ReactionWindowManager Instance { get; private set; }

    [SerializeField] private ReactionUI reactionUI;
    [SerializeField] private float windowDuration = 2.5f; // tüm QTE'ler için ortak süre

    private bool windowOpen = false;
    public bool IsWindowOpen => windowOpen;

    private AbilityData currentAbility;
    private BattleEntity attacker;
    private BattleEntity targetPlayer;
    private Action onDodgeSuccess, onParrySuccess, onCounterSuccess, onWindowExpired;


    // Aktif QTE listeleri
    private List<QTEData> activeQTEs = new List<QTEData>();
    private Dictionary<QTEType, bool> qteSuccess = new Dictionary<QTEType, bool>();

    // Havuzlar
    private readonly KeyCode[] dodgeKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private readonly KeyCode[] parryKeys = { KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.U, KeyCode.I, KeyCode.O };
    private readonly KeyCode[] counterKeys = { KeyCode.C };

    private void Awake()
    {
        Instance = this;
    }

    public void OpenReactionWindow(
    AbilityData ability,
    BattleEntity attackerEntity,
    BattleEntity targetPlayerEntity,
    Action dodgeCallback,
    Action parryCallback,
    Action counterCallback,
    Action expiredCallback)
    {
        if (windowOpen) return;
        windowOpen = true;

        currentAbility = ability;
        attacker = attackerEntity;
        targetPlayer = targetPlayerEntity;

        // Callback'leri sakla
        onDodgeSuccess = dodgeCallback;
        onParrySuccess = parryCallback;
        onCounterSuccess = counterCallback;
        onWindowExpired = expiredCallback;

        activeQTEs.Clear();
        qteSuccess.Clear();

        // Geçerli QTE'leri topla
        foreach (var qte in ability.allowedQTEs)
        {
            if (qte.type == QTEType.Counter)
            {
                if (targetPlayer.CanCounter())   // counter cooldown uygunsa
                {
                    activeQTEs.Add(qte);
                    qteSuccess[qte.type] = false;
                }
            }
            else
            {
                // Dodge ve Parry her zaman geçerli (parry kontrolü yok)
                activeQTEs.Add(qte);
                qteSuccess[qte.type] = false;
            }
        }

        if (activeQTEs.Count == 0)
        {
            // Hiç QTE yok, hemen hasar uygula
            StartCoroutine(ExpireImmediately());
            return;
        }

        // UI'ı hazırla
        reactionUI.ShowQTEOptions(activeQTEs, dodgeKeys, parryKeys, counterKeys);

        StartCoroutine(WindowCoroutine());
    }

    private IEnumerator WindowCoroutine()
    {
        yield return new WaitForSeconds(windowDuration);
        TimeUp();
    }

    private IEnumerator ExpireImmediately()
    {
        yield return null;
        ApplyFullDamage();
    }

    private void TimeUp()
    {
        if (!windowOpen) return;
        windowOpen = false;
        reactionUI.HideAll();

        // Başarılı QTE var mı kontrol et (bir QTE bile başarılı olsa hasar uygulanmaz mı? Yoksa sadece başarılı olanlar mı etkili?)
        // Tasarım: Oyuncu sadece birini başarılı yapabilir, diğerleri başarısız sayılır.
        // Eğer hiçbir QTE tamamlanmadıysa tam hasar.
        bool anySuccess = false;
        foreach (var kv in qteSuccess)
        {
            if (kv.Value)
            {
                anySuccess = true;
                ProcessQTEResult(kv.Key);
                break;  // sadece birini işle
            }
        }

        if (!anySuccess)
        {
            ApplyFullDamage();
        }
    }

    private void ProcessQTEResult(QTEType type)
    {
        Action callback = null;
        switch (type)
        {
            case QTEType.Dodge: callback = onDodgeSuccess; break;
            case QTEType.Parry: callback = onParrySuccess; break;
            case QTEType.Counter: callback = onCounterSuccess; break;
        }
        // Callback'i temizle
        onDodgeSuccess = onParrySuccess = onCounterSuccess = onWindowExpired = null;
        callback?.Invoke();
    }

    private void ApplyFullDamage()
    {
        onWindowExpired?.Invoke();
    }

    public float GetTotalInstantDamage(AbilityData ability)
    {
        float total = 0;
        foreach (var effect in ability.applyEffects)
        {
            if (effect.type == StatusEffectData.EffectType.InstantDamage)
                total += effect.value;
        }
        return total;
    }

    private void EndTurn()
    {
        attacker.EndMyTurn();   // saldırganın turunu bitir
    }

    // Input'lardan gelen bildirimler (UI'dan veya handler'dan)
    public void ReportQTESuccess(QTEType type)
    {
        if (!windowOpen) return;

        StopAllCoroutines();
        windowOpen = false;
        reactionUI.HideAll();

        // Callback'i sakla ve temizle
        Action callback = null;
        switch (type)
        {
            case QTEType.Dodge: callback = onDodgeSuccess; break;
            case QTEType.Parry: callback = onParrySuccess; break;
            case QTEType.Counter: callback = onCounterSuccess; break;
        }
        onDodgeSuccess = onParrySuccess = onCounterSuccess = onWindowExpired = null;
        callback?.Invoke();
    }

    public void ForceFail()
    {
        StopAllCoroutines();
        windowOpen = false;
        reactionUI.HideAll();
        ApplyFullDamage();
    }

    public void ForceClose()
    {
        if (!windowOpen) return;
        StopAllCoroutines();
        windowOpen = false;
        reactionUI.HideAll();
    }
}