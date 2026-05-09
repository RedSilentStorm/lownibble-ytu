using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntity : MonoBehaviour, ITurnTaker
{
    private CharacterData data;
    private float currentHealth;
    private BattleEntity targetEnemy; // düşmana saldırmak için

    public void Initialize(CharacterData characterData, BattleEntity primaryTarget)
    {
        data = characterData;
        currentHealth = data.maxHealth;
        targetEnemy = primaryTarget;
    }

    public float GetSpeed() => data != null ? data.speed : 0f;
    public List<AbilityData> GetAbilities() => data != null ? data.abilities : null;

    public void StartTurn()
    {
        Debug.Log($"{data.characterName} sırası başladı!");
        if (data.isPlayer)
        {
            FightUI.Instance.ShowActions(this, (selectedAbility) =>
            {
                ExecutePlayerAction(selectedAbility);
            });
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);
        // Rastgele yetenek seç
        AbilityData enemyAbility = data.abilities[Random.Range(0, data.abilities.Count)];
        Debug.Log($"{data.characterName} {enemyAbility.abilityName} kullanıyor!");

        // Hedef seç ve saldır
        List<BattleEntity> players = FightManager.PlayerEntities;
        BattleEntity target = players[Random.Range(0, players.Count)];

        yield return new WaitForSeconds(0.3f);
        ApplyAbilityToTarget(enemyAbility, target);
        EndMyTurn();
    }

    private void ExecutePlayerAction(AbilityData ability)
    {
        if (targetEnemy != null)
        {
            ApplyAbilityToTarget(ability, targetEnemy);
        }
        Invoke(nameof(EndMyTurn), 0.5f);
    }

    private void ApplyAbilityToTarget(AbilityData ability, BattleEntity target)
    {
        switch (ability.type)
        {
            case AbilityData.AbilityType.Damage:
                target.TakeDamage(ability.baseDamage);
                Debug.Log($"{data.characterName}, {target.data.characterName} hedefine {ability.baseDamage} hasar verdi!");
                break;
            case AbilityData.AbilityType.Heal:
                // Heal: kendi kendine iyileşme
                Heal(ability.baseDamage);
                Debug.Log($"{data.characterName} kendini {ability.baseDamage} iyileştirdi!");
                break;
            case AbilityData.AbilityType.Poison:
                // Basit poison: ekstra hasar eklenebilir, şimdilik düz hasar
                float poisonDamage = ability.baseDamage + 3; // +3 ekstra
                target.TakeDamage(poisonDamage);
                Debug.Log($"{data.characterName} zehirli saldırı! {poisonDamage} hasar.");
                break;
                // Diğer türler...
        }
    }

    private void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, data.maxHealth);
        Debug.Log($"{data.characterName} iyileşti: {currentHealth}/{data.maxHealth}");
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{data.characterName} canı: {currentHealth}/{data.maxHealth}");
        if (currentHealth <= 0)
        {
            Debug.Log($"{data.characterName} öldü!");
            gameObject.SetActive(false);
            // Savaş bitti bildirimi yapabiliriz, ama şimdilik TurnManager devam eder.
        }
        if (currentHealth <= 0)
        {
            Debug.Log($"{data.characterName} öldü!");
            gameObject.SetActive(false);
            // Eğer ölen düşmansa savaş bitsin
            if (!data.isPlayer)
            {
                FightManager.OnEnemyDefeated();
            }
        }
    }

    private void EndMyTurn()
    {
        Debug.Log($"{data.characterName} sırası bitti.");
        TurnManager.Instance.EndTurn();
    }
}