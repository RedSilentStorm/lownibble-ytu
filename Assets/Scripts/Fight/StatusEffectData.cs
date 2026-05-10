using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Fight/Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    public string effectName;
    public EffectType type;
    public float value;        // Hasar, iyileştirme miktarı, yüzdelik değişim değeri
    public int durationTurns;  // Kaç tur sürecek? (0 = anlık ama süreli olabilir)
    public bool isStackable;   // Aynı etki birden çok kez uygulanabilir mi?
    public bool isInstant;
    public Sprite icon;        // UI için (şimdilik boş)

    public enum EffectType
    {
        Poison,
        Shield,
        AttackUp,
        AttackDown,
        DefenseUp,
        DefenseDown,
        HealOverTime,
        InstantHeal,
        Stun,
        InstantDamage,
        Mark
        // ileride: Cleanse,  vb.
    }
}