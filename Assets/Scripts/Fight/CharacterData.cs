using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "NewCharacter", menuName = "Fight/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public float maxHealth = 100f;
    public float speed = 10f;
    public List<AbilityData> abilities;
    public int counterCooldownTurns = 4;
    public GameObject battlePrefab;
    public bool isPlayer;
    public bool canBreakMark = false;
}