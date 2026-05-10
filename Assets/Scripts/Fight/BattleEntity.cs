using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BattleEntity : MonoBehaviour, ITurnTaker
{
    public CharacterData data;
    private float currentHealth;
    private BattleEntity currentTarget;
    private int counterCooldownRemaining = 0;
    private List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();
    // Cooldown sistemi: Hangi yetenek, kaç tur kaldı
    private Dictionary<AbilityData, int> cooldowns = new Dictionary<AbilityData, int>();

    public void Initialize(CharacterData characterData, BattleEntity primaryTarget)
    {
        data = characterData;
        currentHealth = data.maxHealth;
    }

    public float GetSpeed() => data != null ? data.speed : 0f;
    public List<AbilityData> GetAbilities() => data != null ? data.abilities : null;

    public void StartTurn()
    {
        if (!gameObject.activeSelf)
        {
            EndMyTurn();
            return;
        }
        if (ReactionWindowManager.Instance.IsWindowOpen)
        {
            ReactionWindowManager.Instance.ForceClose();
        }
        ProcessEffects(); // Tur başında efektleri işle
        ReduceCooldowns();
        ReduceCounterCooldown();

        Debug.Log($"{data.characterName} sırası başladı!");
        if (data.isPlayer)
        {
            // Önce hedef seç
            List<BattleEntity> targets = FightManager.EnemyEntities.FindAll(e => e.gameObject.activeSelf);
            if (targets.Count == 0)
            {
                EndMyTurn(); // düşman kalmadıysa
                return;
            }
            FightUI.Instance.ShowTargetSelection(targets, (selectedTarget) =>
            {
                currentTarget = selectedTarget;
                StartCoroutine(ShowActionsAfterFrame(selectedTarget));
            });
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    System.Collections.IEnumerator ShowActionsAfterFrame(BattleEntity selectedTarget)
    {
        yield return null; // 1 kare bekle
        FightUI.Instance.ShowActions(this, (selectedAbility) =>
        {
            ExecutePlayerAction(selectedAbility);
        });
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        // Kullanılabilir yetenekleri al
        List<AbilityData> available = data.abilities.FindAll(a => !IsOnCooldown(a));
        if (available.Count == 0)
        {
            Debug.Log($"{data.characterName} kullanılabilir yetenek yok, tur atlandı!");
            EndMyTurn();
            yield break;
        }

        AbilityData enemyAbility = available[Random.Range(0, available.Count)];
        Debug.Log($"{data.characterName} {enemyAbility.abilityName} kullanıyor!");

        // Hedef seç
        List<BattleEntity> players = FightManager.PlayerEntities.FindAll(p => p.gameObject.activeSelf);
        if (players.Count == 0) { EndMyTurn(); yield break; }
        BattleEntity target = players[Random.Range(0, players.Count)];

        // QTE penceresi açılacak mı?
        if (enemyAbility.allowedQTEs != null && enemyAbility.allowedQTEs.Count > 0)
        {
            Debug.Log($"{data.characterName} QTE penceresi açıyor: {enemyAbility.abilityName}");
            // QTE'yi başlat, bu coroutine'i burada keselim
            ReactionWindowManager.Instance.OpenReactionWindow(
                enemyAbility,
                this,   // saldırgan
                target,
                dodgeCallback: () =>
                {
                    Debug.Log("Dodge başarılı! Hasar yok.");
                    EndMyTurn();
                },
                parryCallback: () =>
                {
                    float reflected = ReactionWindowManager.Instance.GetTotalInstantDamage(enemyAbility) * 0.5f;
                    // Düşman kendine hasar alır (hedef yansıtır)
                    Debug.Log($"Parry başarılı! {reflected} hasar düşmana yansıdı.");
                    TakeDamage(reflected);
                    EndMyTurn();
                },
                counterCallback: () =>
                {
                    Debug.Log("Counter başarılı!");
                    // Basit bir karşı saldırı (ileride oyuncu seçimi eklenecek)
                    float counterDmg = target.GetAutoCounterDamage();
                    TakeDamage(counterDmg);
                    target.StartCounterCooldown();
                    EndMyTurn();
                },
                expiredCallback: () =>
                {
                    Debug.Log("QTE kaçırıldı, tam hasar uygulanıyor.");
                    ApplyAbilityToTarget(enemyAbility, target);
                    EndMyTurn();
                }
            );
            yield break; // QTE açıldı, bu coroutine'i bitir
        }
        else
        {
            // Normal saldırı (QTE yok)
            yield return new WaitForSeconds(0.3f);
            ApplyAbilityToTarget(enemyAbility, target);
            EndMyTurn();
        }
    }

    private void ExecutePlayerAction(AbilityData ability)
    {
        if (currentTarget != null)
        {
            ApplyAbilityToTarget(ability, currentTarget);
        }
        Invoke(nameof(EndMyTurn), 0.5f);
    }

    public void ApplyAbilityToTarget(AbilityData ability, BattleEntity target)
    {
        if (ability.applyEffects == null || ability.applyEffects.Count == 0) return;

        Debug.Log($"{data.characterName} {ability.abilityName} yeteneğini {target.data.characterName} hedefine uyguluyor!");
        this.StartCooldown(ability);

        foreach (var effect in ability.applyEffects)
        {
            if (effect.isInstant)
            {
                target.ApplyInstantEffect(effect, this); // this = kaynak
            }
            else
            {
                target.ApplyOverTimeEffect(effect);
            }
        }
    }

    public void ApplyInstantEffect(StatusEffectData effectData, BattleEntity source)
    {
        switch (effectData.type)
        {
            case StatusEffectData.EffectType.InstantHeal:
                Heal(effectData.value);
                Debug.Log($"{data.characterName} anında {effectData.value} can kazandı.");
                break;
            case StatusEffectData.EffectType.InstantDamage:
                float attackMulti = source != null ? source.GetAttackMultiplier() : 1f;
                float finalDamage = effectData.value * attackMulti;

                // Mark kontrolü ve çarpan uygulaması
                if (HasMark() && source != null && source.data.canBreakMark)
                {
                    ActiveStatusEffect mark = activeEffects.Find(e => e.data.type == StatusEffectData.EffectType.Mark);
                    if (mark != null)
                    {
                        finalDamage *= mark.data.value; // çarpan
                        activeEffects.Remove(mark);
                        Debug.Log($"{source.data.characterName}, {data.characterName} üzerindeki Markı patlattı! Hasar {mark.data.value}x ile çarpıldı: {finalDamage}");
                    }
                }

                TakeDamage(finalDamage);
                Debug.Log($"{source.data.characterName}, {data.characterName} hedefine {finalDamage} hasar verdi!");
                break;
                // case StatusEffectData.EffectType.Shield: // Örnek: Kalkan da anlık olabilir
                //     ActiveStatusEffect shield = new ActiveStatusEffect(effectData);
                //     shield.remainingTurns = effectData.durationTurns; // süreli ama anında eklenir
                //     activeEffects.Add(shield);
                //     Debug.Log($"{data.characterName} kalkan kazandı: {effectData.value}");
                //     break;
        }
    }

    public void ApplyOverTimeEffect(StatusEffectData effectData)
    {
        // Mevcut ApplyEffect'in sadece süreli ekleme kısmı
        ActiveStatusEffect existing = activeEffects.Find(e => e.data == effectData);
        if (existing != null)
        {
            if (effectData.isStackable) existing.stacks++;
            existing.remainingTurns = effectData.durationTurns;
        }
        else
        {
            activeEffects.Add(new ActiveStatusEffect(effectData));
        }
        Debug.Log($"{data.characterName} üzerinde {effectData.effectName} etkisi başladı. Kalan tur: {effectData.durationTurns}");
    }

    public void ProcessEffects()
    {
        List<ActiveStatusEffect> expired = new List<ActiveStatusEffect>();

        foreach (var active in activeEffects)
        {
            switch (active.data.type)
            {
                case StatusEffectData.EffectType.Poison:
                    float poisonDamage = active.data.value * active.stacks;
                    TakeDamage(poisonDamage);
                    Debug.Log($"{data.characterName} zehir hasarı aldı: {poisonDamage}");
                    break;
                case StatusEffectData.EffectType.HealOverTime:
                    Heal(active.data.value * active.stacks);
                    break;
                    // Shield, AttackUp/Down gibi pasifler işlem gerektirmez
            }

            active.remainingTurns--;
            if (active.remainingTurns <= 0)
            {
                expired.Add(active);
            }
        }

        foreach (var exp in expired)
        {
            activeEffects.Remove(exp);
            Debug.Log($"{data.characterName} üzerindeki {exp.data.effectName} etkisi sona erdi.");
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"{data.characterName} üzerindeki tüm efektler:");
        foreach (var effect in activeEffects)
        {
            Debug.Log($" - {effect.data.effectName} (Kalan tur: {effect.remainingTurns})");
        }
        // Kalkan kontrolü
        float shieldValue = 0;
        ActiveStatusEffect shieldEffect = activeEffects.Find(e => e.data.type == StatusEffectData.EffectType.Shield);
        if (shieldEffect != null)
        {
            shieldValue = shieldEffect.data.value * shieldEffect.stacks;
            activeEffects.Remove(shieldEffect);
            Debug.Log($"{data.characterName} kalkanı {shieldValue} hasarı engelledi.");
        }

        float finalDamage = Mathf.Max(0, amount - shieldValue);

        // DefenseDown zayıflığı varsa hasarı artır
        ActiveStatusEffect defDown = activeEffects.Find(e => e.data.type == StatusEffectData.EffectType.DefenseDown);
        if (defDown != null)
        {
            Debug.Log($"{data.characterName} savunması azaldı! Hasar artıyor.");
            finalDamage *= (1 + defDown.data.value * defDown.stacks);
        }
        // DefenseUp varsa azalt
        ActiveStatusEffect defUp = activeEffects.Find(e => e.data.type == StatusEffectData.EffectType.DefenseUp);
        if (defUp != null)
        {
            finalDamage *= (1 - defUp.data.value * defUp.stacks);
        }

        currentHealth -= finalDamage;
        Debug.Log($"{data.characterName} {finalDamage} hasar aldı. Can: {currentHealth}/{data.maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{data.characterName} öldü!");
            gameObject.SetActive(false);
            if (!data.isPlayer)
            {
                FightManager.CheckAllEnemiesDefeated();
            }
        }
    }

    public void ReduceCooldowns()
    {
        // Anahtarları ayrı bir listeye kopyala
        List<AbilityData> keys = new List<AbilityData>(cooldowns.Keys);
        List<AbilityData> expired = new List<AbilityData>();

        foreach (var key in keys)
        {
            cooldowns[key]--;
            if (cooldowns[key] <= 0)
            {
                expired.Add(key);
                Debug.Log($"{data.characterName} için {key.abilityName} cooldown'ı bitti.");
            }
        }

        foreach (var ability in expired)
        {
            cooldowns.Remove(ability);
        }
    }

    public void StartCooldown(AbilityData ability)
    {
        if (ability.cooldownTurns <= 0) return;

        cooldowns[ability] = ability.cooldownTurns;
        Debug.Log($"{data.characterName}, {ability.abilityName} yeteneğini {ability.cooldownTurns} tur cooldown'a soktu.");
    }

    public bool IsOnCooldown(AbilityData ability)
    {
        return cooldowns.ContainsKey(ability) && cooldowns[ability] > 0;
    }

    public int GetCooldownRemaining(AbilityData ability)
    {
        if (cooldowns.TryGetValue(ability, out int remaining))
            return remaining;
        return 0;
    }

    public void ReduceCounterCooldown()
    {
        if (counterCooldownRemaining > 0)
        {
            counterCooldownRemaining--;
            if (counterCooldownRemaining == 0)
                Debug.Log($"{data.characterName} Counter hazır.");
        }
    }

    public bool CanCounter() => counterCooldownRemaining <= 0;
    public void StartCounterCooldown()
    {
        counterCooldownRemaining = data.counterCooldownTurns;
    }

    public float GetAttackMultiplier()
    {
        float multiplier = 1f;
        foreach (var effect in activeEffects)
        {
            if (effect.data.type == StatusEffectData.EffectType.AttackUp)
                multiplier += effect.data.value * effect.stacks;
            else if (effect.data.type == StatusEffectData.EffectType.AttackDown)
                multiplier -= effect.data.value * effect.stacks;
        }
        return Mathf.Max(0, multiplier);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, data.maxHealth);
        Debug.Log($"{data.characterName} iyileşti: {currentHealth}/{data.maxHealth}");
    }

    public void EndMyTurn()
    {
        Debug.Log($"{data.characterName} sırası bitti.");
        TurnManager.Instance.EndTurn();
    }

    public float GetAutoCounterDamage()
    {
        // Şimdilik sabit bir değer döndür, ileride CharacterData’dan alırız PLACEHOLDER
        return 15f;
    }

    public bool HasMark()
    {
        return activeEffects.Exists(e => e.data.type == StatusEffectData.EffectType.Mark);
    }

    public void ConsumeMark(BattleEntity attacker)
    {
        ActiveStatusEffect mark = activeEffects.Find(e => e.data.type == StatusEffectData.EffectType.Mark);
        if (mark != null && attacker.data.canBreakMark)
        {
            float bonusDamage = mark.data.value; // Mark efektinin "value" alanı bonus hasar olarak kullanılır
            activeEffects.Remove(mark);
            // Bonus hasarı hemen uygulayalım (instant damage gibi)
            TakeDamage(bonusDamage);
            Debug.Log($"{attacker.data.characterName}, {data.characterName} üzerindeki Markı patlattı! {bonusDamage} ek hasar!");
        }
    }
}

[System.Serializable]
public class ActiveStatusEffect
{
    public StatusEffectData data;
    public int remainingTurns;
    public int stacks;

    public ActiveStatusEffect(StatusEffectData effectData)
    {
        data = effectData;
        remainingTurns = effectData.durationTurns;
        stacks = 1;
    }
}