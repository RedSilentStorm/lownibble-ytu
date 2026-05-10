using System.Collections.Generic;
using UnityEngine;

public enum QTEType { Dodge, Parry, Counter }
public enum QTEComplexity { Simple, Medium, Combo }
public enum TargetType { Enemy, Ally}


[System.Serializable]
public class QTEData
{
    public QTEType type;
    public QTEComplexity complexity;
}

[CreateAssetMenu(fileName = "NewAbility", menuName = "Fight/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public List<StatusEffectData> applyEffects;
    public int cooldownTurns = 0;
    public List<QTEData> allowedQTEs = new List<QTEData>();
    public string animationTrigger;
    public TargetType targetType;
}