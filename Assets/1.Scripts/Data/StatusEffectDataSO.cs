using UnityEngine;

/// <summary>
/// 상태 효과(버프, 디버프)의 속성을 정의하는 ScriptableObject입니다.
/// 독, 화상, 능력치 강화 등 다양한 효과를 데이터로 만들어 관리할 수 있습니다.
/// </summary>
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

    [Header("능력치 변경 효과")]
    public float damageRatioBonus;
    public float attackSpeedRatioBonus;
    public float moveSpeedRatioBonus;
    public float healthRatioBonus;
    public float critRateRatioBonus;
    public float critDamageRatioBonus;

    [Header("지속 피해/회복 효과")]
    public float damageOverTime;
    public float healOverTime;

    /// <summary>
    /// 대상 캐릭터에게 이 상태 효과의 능력치 보너스를 적용합니다.
    /// </summary>
    public void ApplyEffect(CharacterStats targetStats)
    {
        if (targetStats == null) return;

        targetStats.buffDamageRatio += damageRatioBonus;
        targetStats.buffAttackSpeedRatio += attackSpeedRatioBonus;
        targetStats.buffMoveSpeedRatio += moveSpeedRatioBonus;
        targetStats.buffHealthRatio += healthRatioBonus;
        targetStats.buffCritRateRatio += critRateRatioBonus;
        targetStats.buffCritDamageRatio += critDamageRatioBonus;

        targetStats.CalculateFinalStats();
    }

    /// <summary>
    /// 대상 캐릭터에게서 이 상태 효과의 능력치 보너스를 제거합니다.
    /// </summary>
    public void RemoveEffect(CharacterStats targetStats)
    {
        if (targetStats == null) return;

        targetStats.buffDamageRatio -= damageRatioBonus;
        targetStats.buffAttackSpeedRatio -= attackSpeedRatioBonus;
        targetStats.buffMoveSpeedRatio -= moveSpeedRatioBonus;
        targetStats.buffHealthRatio -= healthRatioBonus;
        targetStats.buffCritRateRatio -= critRateRatioBonus;
        targetStats.buffCritDamageRatio -= critDamageRatioBonus;

        targetStats.CalculateFinalStats();
    }
}