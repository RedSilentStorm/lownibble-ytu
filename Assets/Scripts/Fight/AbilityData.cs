using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Fight/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public float baseDamage = 10f;
    public AbilityType type = AbilityType.Damage;

    public enum AbilityType
    {
        Damage,
        Heal,
        Poison,
        Buff
        // istediğin kadar ekle
    }
}