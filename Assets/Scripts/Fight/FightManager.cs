using System.Collections.Generic;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] private CharacterData playerDataA;
    [SerializeField] private CharacterData playerDataB;
    [SerializeField] private Transform playerSpawnA;
    [SerializeField] private Transform playerSpawnB;
    [SerializeField] private CharacterData[] enemyDatas;
    [SerializeField] private Transform[] enemySpawns;
    [SerializeField] private GameObject healthDisplayPrefab;

    public static List<BattleEntity> PlayerEntities { get; private set; } = new List<BattleEntity>();
    public static List<BattleEntity> EnemyEntities { get; private set; } = new List<BattleEntity>();

    private void Start()
    {
        // Oyuncuları oluştur
        GameObject playerObjA = Instantiate(playerDataA.battlePrefab, playerSpawnA);
        BattleEntity playerA = playerObjA.GetComponent<BattleEntity>();
        playerA.Initialize(playerDataA, null);
        if (healthDisplayPrefab != null)
        {
            var attachA = playerObjA.GetComponent<AttachHealthDisplay>();
            if (attachA == null) attachA = playerObjA.AddComponent<AttachHealthDisplay>();
            attachA.healthDisplayPrefab = healthDisplayPrefab;
        }

        GameObject playerObjB = Instantiate(playerDataB.battlePrefab, playerSpawnB);
        BattleEntity playerB = playerObjB.GetComponent<BattleEntity>();
        playerB.Initialize(playerDataB, null);
        if (healthDisplayPrefab != null)
        {
            var attachB = playerObjB.GetComponent<AttachHealthDisplay>();
            if (attachB == null) attachB = playerObjB.AddComponent<AttachHealthDisplay>();
            attachB.healthDisplayPrefab = healthDisplayPrefab;
        }

        PlayerEntities.Clear();
        PlayerEntities.Add(playerA);
        PlayerEntities.Add(playerB);

        // Düşmanları oluştur
        EnemyEntities.Clear();
        for (int i = 0; i < enemyDatas.Length; i++)
        {
            if (enemyDatas[i] == null || enemySpawns[i] == null) continue;

            GameObject enemyObj = Instantiate(enemyDatas[i].battlePrefab, enemySpawns[i]);
            BattleEntity enemy = enemyObj.GetComponent<BattleEntity>();
            enemy.Initialize(enemyDatas[i], null);
            // Attach health display for enemies if prefab provided
            if (healthDisplayPrefab != null)
            {
                var attach = enemyObj.GetComponent<AttachHealthDisplay>();
                if (attach == null) attach = enemyObj.AddComponent<AttachHealthDisplay>();
                attach.healthDisplayPrefab = healthDisplayPrefab;
            }
            EnemyEntities.Add(enemy);
        }

        // TurnManager'a hepsini kaydet
        foreach (var p in PlayerEntities) TurnManager.Instance.AddTurnTaker(p);
        foreach (var e in EnemyEntities) TurnManager.Instance.AddTurnTaker(e);

        TurnManager.Instance.StartBattle();
    }
    public static void CheckAllEnemiesDefeated()
    {
        bool allDead = EnemyEntities.TrueForAll(e => e == null || e.IsDead);
        if (allDead && EnemyEntities.Count > 0)
        {
            Debug.Log("Tüm düşmanlar yenildi! Comic'e geçiliyor.");
            GameManager.Instance.SwitchToComic();
        }
    }
}