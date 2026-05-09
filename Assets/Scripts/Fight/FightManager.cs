using System.Collections.Generic;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] private CharacterData playerDataA;
    [SerializeField] private CharacterData playerDataB;
    [SerializeField] private CharacterData enemyData;
    [SerializeField] private Transform playerSpawnA;
    [SerializeField] private Transform playerSpawnB;
    [SerializeField] private Transform enemySpawn;

    public static List<BattleEntity> PlayerEntities { get; private set; } = new List<BattleEntity>();
    public static BattleEntity EnemyEntity { get; private set; }

    private void Start()
    {
        // Oyuncu A
        GameObject playerObjA = Instantiate(playerDataA.battlePrefab, playerSpawnA);
        BattleEntity playerA = playerObjA.GetComponent<BattleEntity>();

        // Oyuncu B
        GameObject playerObjB = Instantiate(playerDataB.battlePrefab, playerSpawnB);
        BattleEntity playerB = playerObjB.GetComponent<BattleEntity>();

        // Düşman
        GameObject enemyObj = Instantiate(enemyData.battlePrefab, enemySpawn);
        BattleEntity enemy = enemyObj.GetComponent<BattleEntity>();

        if (playerA == null || playerB == null || enemy == null)
        {
            Debug.LogError("Battle prefablar BattleEntity scriptine sahip olmalı!");
            return;
        }

        // Düşmanı hedef göstererek initialize (oyuncular için hedef düşman)
        playerA.Initialize(playerDataA, enemy);
        playerB.Initialize(playerDataB, enemy);

        // Düşmanın hedefi oyunculardan biri olacak, ama biz hedef listesini rastgele seçeceğiz.
        // İstersen enemy.Initialize(enemyData, null) yapıp hedefi sonra belirleyebilirsin.
        enemy.Initialize(enemyData, null); // Hedefi düşman kendi turunda belirleyecek

        // Listeleri doldur
        PlayerEntities.Clear();
        PlayerEntities.Add(playerA);
        PlayerEntities.Add(playerB);
        EnemyEntity = enemy;

        // TurnManager'a kaydet
        TurnManager.Instance.AddTurnTaker(playerA);
        TurnManager.Instance.AddTurnTaker(playerB);
        TurnManager.Instance.AddTurnTaker(enemy);

        TurnManager.Instance.StartBattle();
    }
    public static void OnEnemyDefeated()
    {
        Debug.Log("Düşman yenildi! Comic sahnesine geçiliyor.");
        GameManager.Instance.SwitchToComic();
    }
}