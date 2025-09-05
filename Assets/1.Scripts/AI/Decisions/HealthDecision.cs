// 경로: ./TTttTT/Assets/1/Scripts/AI/Decisions/HealthDecision.cs
using UnityEngine;

/// <summary>
/// [신규 결정 부품] 자신의 현재 체력 비율을 기준으로 판단하는 '센서'입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Health")]
public class HealthDecision : Decision
{
    [Tooltip("판단 기준이 되는 체력의 비율(%)입니다. 30으로 설정하면 30%를 의미합니다.")]
    [Range(0f, 100f)]
    public float healthPercentageThreshold = 30f;

    [Tooltip("체크: 체력이 기준치 '이하'일 때 참 / 체크 해제: 체력이 기준치 '이상'일 때 참")]
    public bool triggerWhenBelow = true;

    public override bool Decide(MonsterController monster)
    {
        if (monster.monsterStats == null) return false;

        // 현재 체력 비율을 계산합니다 (0.0 ~ 1.0 사이의 값)
        float currentHealthRatio = monster.monsterStats.CurrentHealth / monster.monsterStats.FinalMaxHealth;
        // Inspector에서 입력한 퍼센트 값을 비율로 변환합니다.
        float thresholdRatio = healthPercentageThreshold / 100f;

        if (triggerWhenBelow)
        {
            // 현재 체력이 기준치 '이하'일 때 true를 반환합니다.
            return currentHealthRatio <= thresholdRatio;
        }
        else
        {
            // 현재 체력이 기준치 '이상'일 때 true를 반환합니다.
            return currentHealthRatio >= thresholdRatio;
        }
    }
}