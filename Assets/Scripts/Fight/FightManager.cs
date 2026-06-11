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
        // Başlangıçta inspector'da ayarlı verilerle spawn et
        SpawnFight(playerDataA, playerDataB, new List<CharacterData>(enemyDatas));
    }

    /// <summary>
    /// Helper: find CharacterData asset by its characterName or asset name
    /// Uses Resources.FindObjectsOfTypeAll to locate ScriptableObject assets in editor/play mode.
    /// </summary>
    private CharacterData FindCharacterByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var all = Resources.FindObjectsOfTypeAll<CharacterData>();
        foreach (var cd in all)
        {
            if (cd == null) continue;
            if (string.Equals(cd.characterName, name, System.StringComparison.OrdinalIgnoreCase)) return cd;
            if (string.Equals(cd.name, name, System.StringComparison.OrdinalIgnoreCase)) return cd;
        }
        return null;
    }

    /// <summary>
    /// Spawn players and enemies from provided CharacterData.
    /// Clears any existing entities first.
    /// </summary>
    public void SpawnFight(CharacterData pA, CharacterData pB, List<CharacterData> enemies)
    {
        // Destroy existing gameobjects
        foreach (var pe in PlayerEntities)
        {
            if (pe != null) Destroy(pe.gameObject);
        }
        foreach (var ee in EnemyEntities)
        {
            if (ee != null) Destroy(ee.gameObject);
        }

        PlayerEntities.Clear();
        EnemyEntities.Clear();

        // Spawn players if provided
        BattleEntity playerA = null;
        BattleEntity playerB = null;

        if (pA != null && playerSpawnA != null)
        {
            var goA = Instantiate(pA.battlePrefab, playerSpawnA);
            playerA = goA.GetComponent<BattleEntity>();
            playerA.Initialize(pA, null);
            if (healthDisplayPrefab != null)
            {
                var attach = goA.GetComponent<AttachHealthDisplay>() ?? goA.AddComponent<AttachHealthDisplay>();
                attach.healthDisplayPrefab = healthDisplayPrefab;
            }
            PlayerEntities.Add(playerA);
            TurnManager.Instance.AddTurnTaker(playerA);
        }

        if (pB != null && playerSpawnB != null)
        {
            var goB = Instantiate(pB.battlePrefab, playerSpawnB);
            playerB = goB.GetComponent<BattleEntity>();
            playerB.Initialize(pB, null);
            if (healthDisplayPrefab != null)
            {
                var attach = goB.GetComponent<AttachHealthDisplay>() ?? goB.AddComponent<AttachHealthDisplay>();
                attach.healthDisplayPrefab = healthDisplayPrefab;
            }
            PlayerEntities.Add(playerB);
            TurnManager.Instance.AddTurnTaker(playerB);
        }

        // Spawn enemies into available enemySpawns
        if (enemies != null && enemySpawns != null)
        {
            int spawnIndex = 0;
            for (int i = 0; i < enemies.Count && spawnIndex < enemySpawns.Length; i++)
            {
                var ed = enemies[i];
                if (ed == null) continue;
                var spawn = enemySpawns[spawnIndex++];
                if (spawn == null) continue;
                var eg = Instantiate(ed.battlePrefab, spawn);
                var enemy = eg.GetComponent<BattleEntity>();
                enemy.Initialize(ed, null);
                if (healthDisplayPrefab != null)
                {
                    var attach = eg.GetComponent<AttachHealthDisplay>() ?? eg.AddComponent<AttachHealthDisplay>();
                    attach.healthDisplayPrefab = healthDisplayPrefab;
                }
                EnemyEntities.Add(enemy);
                TurnManager.Instance.AddTurnTaker(enemy);
            }
        }

        // Start or restart the TurnManager battle
        TurnManager.Instance.StartBattle();
    }
    public static void CheckAllEnemiesDefeated()
    {
        bool allDead = EnemyEntities.TrueForAll(e => e == null || e.IsDead);
        if (allDead && EnemyEntities.Count > 0)
        {
            // Akış: Comic -> Fight. Comic'e geri dönülürse baştan başlayıp tekrar
            // Fight'a geçer ve sonsuz döngü oluşur; bu yüzden burada bitiriyoruz.
            Debug.Log("Tüm düşmanlar yenildi! Oyun tamamlandı.");
        }
    }

    /// <summary>
    /// Apply configuration string passed from other systems (e.g. Comic/GameManager)
    /// Provides a simple hook to modify the freshly-loaded Fight scene.
    /// Example: "clearEnemies" will remove all spawned enemies.
    /// </summary>
    public void ApplyFightConfig(string config)
    {
        Debug.Log($"[FightManager] ApplyFightConfig: {config}");
        if (string.IsNullOrEmpty(config)) return;
        // Basit tanımlı konfigürasyonlar: fight1, fight2, fight3
        if (config == "fight1")
        {
            // oyuncu: Yusef ; düşmanlar: 2x Skeleton, 1x Golem
            var p = FindCharacterByName("Yusef");
            var sk = FindCharacterByName("Skeleton");
            var golem = FindCharacterByName("Golem");
            var enemies = new List<CharacterData>();
            if (sk != null) { enemies.Add(sk); enemies.Add(sk); }
            if (golem != null) enemies.Add(golem);
            SpawnFight(p, null, enemies);
        }
        else if (config == "fight2")
        {
            // oyuncu: Alee ; düşmanlar: Skeleton, Golem, 2x Necromancer
            var p = FindCharacterByName("Alee");
            var sk = FindCharacterByName("Skeleton");
            var golem = FindCharacterByName("Golem");
            var nec = FindCharacterByName("Necromancer");
            var enemies = new List<CharacterData>();
            if (sk != null) enemies.Add(sk);
            if (golem != null) enemies.Add(golem);
            if (nec != null) { enemies.Add(nec); enemies.Add(nec); }
            SpawnFight(p, null, enemies);
        }
        else if (config == "fight3")
        {
            // oyuncu: Isro ; düşmanlar: 2x Golem, 1x Necromancer
            var p = FindCharacterByName("Isro");
            var golem = FindCharacterByName("Golem");
            var nec = FindCharacterByName("Necromancer");
            var enemies = new List<CharacterData>();
            if (golem != null) { enemies.Add(golem); enemies.Add(golem); }
            if (nec != null) enemies.Add(nec);
            SpawnFight(p, null, enemies);
        }
        else if (config == "clearEnemies")
        {
            for (int i = EnemyEntities.Count - 1; i >= 0; i--)
            {
                var ent = EnemyEntities[i];
                if (ent != null) Destroy(ent.gameObject);
            }
            EnemyEntities.Clear();
            Debug.Log("[FightManager] Tüm düşmanlar temizlendi (clearEnemies).");
        }
        else
        {
            Debug.Log($"[FightManager] Bilinmeyen config: {config} - özel davranış uygulanmadı.");
        }
    }
}