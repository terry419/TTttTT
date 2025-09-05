// 경로: ./TTttTT/Assets/1.Scripts/Data/StatusEffectDataSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectData_", menuName = "GameData/StatusEffectData")]
public class StatusEffectDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string effectId;
    public string effectName;
    public Sprite icon;

    [Header("효과 속성")]
    public float duration;
    public bool isBuff;

    [Header("능력치 변경 효과 (백분율, %)")]
    public float damageRatioBonus;
    public float attackSpeedRatioBonus;
    public float moveSpeedRatioBonus;
    public float healthRatioBonus;
    public float critRateRatioBonus;
    public float critDamageRatioBonus;

    [Header("지속 피해/회복 효과")]
    public float damageOverTime;
    public float healOverTime;
}